using Godot;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;

namespace RoguelikeGame.Core
{
    public enum ConfigFormat
    {
        Json,
        Bytes
    }

    public static class ConfigLoader
    {
        private const string CONFIG_DATA_PATH = "res://GameModes/base_game/Config/Data/";
        private const string CONFIG_COMPILED_PATH = "res://GameModes/base_game/Config/Compiled/";
        private const string JSON_EXTENSION = ".json";
        private const string BYTES_EXTENSION = ".bytes";

        private static bool _useCompiledConfig = false;
        private static readonly Dictionary<string, object> _configCache = new();

        public static bool UseCompiledConfig
        {
            get => _useCompiledConfig;
            set
            {
                _useCompiledConfig = value;
                _configCache.Clear();
            }
        }

        public static T LoadConfig<T>(string configName) where T : class
        {
            string cacheKey = $"{configName}_{typeof(T).Name}";

            if (_configCache.TryGetValue(cacheKey, out var cached))
            {
                return cached as T;
            }

            T config = null;

            if (_useCompiledConfig)
            {
                config = LoadFromBytes<T>(configName);
            }

            if (config == null)
            {
                config = LoadFromJson<T>(configName);
            }

            if (config != null)
            {
                _configCache[cacheKey] = config;
            }

            return config;
        }

        public static T LoadFromJson<T>(string configName) where T : class
        {
            string path = CONFIG_DATA_PATH + configName + JSON_EXTENSION;

            if (!ResourceLoader.Exists(path))
            {
                GD.PrintErr($"[ConfigLoader] JSON config not found: {path}");
                return null;
            }

            try
            {
                var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
                if (file == null)
                {
                    GD.PrintErr($"[ConfigLoader] Failed to open JSON file: {path}");
                    return null;
                }

                string jsonContent = file.GetAsText();
                file.Close();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };

                T config = JsonSerializer.Deserialize<T>(jsonContent, options);

                GD.Print($"[ConfigLoader] Loaded JSON config: {configName}");
                return config;
            }
            catch (Exception e)
            {
                GD.PrintErr($"[ConfigLoader] Error loading JSON config {configName}: {e.Message}");
                return null;
            }
        }

        public static T LoadFromBytes<T>(string configName) where T : class
        {
            string path = CONFIG_COMPILED_PATH + configName + BYTES_EXTENSION;

            if (!ResourceLoader.Exists(path))
            {
                GD.Print($"[ConfigLoader] Bytes config not found: {path}, falling back to JSON");
                return null;
            }

            try
            {
                var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
                if (file == null)
                {
                    GD.PrintErr($"[ConfigLoader] Failed to open bytes file: {path}");
                    return null;
                }

                byte[] compressedData = file.GetBuffer((long)file.GetLength());
                file.Close();

                string jsonContent = DecompressString(compressedData);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };

                T config = JsonSerializer.Deserialize<T>(jsonContent, options);

                GD.Print($"[ConfigLoader] Loaded bytes config: {configName}");
                return config;
            }
            catch (Exception e)
            {
                GD.PrintErr($"[ConfigLoader] Error loading bytes config {configName}: {e.Message}");
                return null;
            }
        }

        public static bool CompileConfigToJson(string configName, object data)
        {
            string jsonPath = CONFIG_DATA_PATH + configName + JSON_EXTENSION;

            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                string jsonContent = JsonSerializer.Serialize(data, options);

                using var file = Godot.FileAccess.Open(jsonPath, Godot.FileAccess.ModeFlags.Write);
                if (file == null)
                {
                    GD.PrintErr($"[ConfigLoader] Failed to create JSON file: {jsonPath}");
                    return false;
                }

                file.StoreString(jsonContent);
                file.Close();

                GD.Print($"[ConfigLoader] Saved JSON config: {configName}");
                return true;
            }
            catch (Exception e)
            {
                GD.PrintErr($"[ConfigLoader] Error saving JSON config {configName}: {e.Message}");
                return false;
            }
        }

        public static bool CompileConfigToBytes(string configName)
        {
            string jsonPath = CONFIG_DATA_PATH + configName + JSON_EXTENSION;
            string bytesPath = CONFIG_COMPILED_PATH + configName + BYTES_EXTENSION;

            if (!ResourceLoader.Exists(jsonPath))
            {
                GD.PrintErr($"[ConfigLoader] Source JSON not found: {jsonPath}");
                return false;
            }

            try
            {
                var jsonFile = Godot.FileAccess.Open(jsonPath, Godot.FileAccess.ModeFlags.Read);
                if (jsonFile == null)
                {
                    GD.PrintErr($"[ConfigLoader] Failed to open JSON file: {jsonPath}");
                    return false;
                }

                string jsonContent = jsonFile.GetAsText();
                jsonFile.Close();

                byte[] compressedData = CompressString(jsonContent);

                using var bytesFile = Godot.FileAccess.Open(bytesPath, Godot.FileAccess.ModeFlags.Write);
                if (bytesFile == null)
                {
                    GD.PrintErr($"[ConfigLoader] Failed to create bytes file: {bytesPath}");
                    return false;
                }

                bytesFile.StoreBuffer(compressedData);
                bytesFile.Close();

                GD.Print($"[ConfigLoader] Compiled config to bytes: {configName} (size: {compressedData.Length} bytes)");
                return true;
            }
            catch (Exception e)
            {
                GD.PrintErr($"[ConfigLoader] Error compiling config {configName}: {e.Message}");
                return false;
            }
        }

        public static bool CompileAllConfigs()
        {
            string[] configNames = { "cards", "characters", "enemies", "relics", "potions", "events", "audio", "effects" };

            int successCount = 0;
            foreach (var configName in configNames)
            {
                if (CompileConfigToBytes(configName))
                {
                    successCount++;
                }
            }

            GD.Print($"[ConfigLoader] Compiled {successCount}/{configNames.Length} configs");
            return successCount == configNames.Length;
        }

        private static byte[] CompressString(string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);
            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionLevel.Optimal))
            {
                gzip.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        private static string DecompressString(byte[] compressedData)
        {
            using var input = new MemoryStream(compressedData);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            gzip.CopyTo(output);
            return Encoding.UTF8.GetString(output.ToArray());
        }

        public static void ClearCache()
        {
            _configCache.Clear();
            GD.Print("[ConfigLoader] Cache cleared");
        }

        public static bool ValidateConfig<T>(T config) where T : class
        {
            if (config == null)
            {
                GD.PrintErr($"[ConfigLoader] Config validation failed: config is null");
                return false;
            }

            return true;
        }
    }
}
