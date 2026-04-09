using Godot;
using System;
using RoguelikeGame.Core;

namespace RoguelikeGame.Editor
{
    [Tool]
    public partial class ConfigCompiler : Node
    {
        public override void _Ready()
        {
            if (Engine.IsEditorHint())
            {
                GD.Print("[ConfigCompiler] Ready to compile configs");
            }
        }

        public static void CompileAllConfigs()
        {
            GD.Print("========================================");
            GD.Print("[ConfigCompiler] Starting config compilation...");
            GD.Print("========================================");

            string[] configNames = { "cards", "characters", "enemies", "relics", "potions", "events", "audio", "effects" };

            int successCount = 0;
            int failCount = 0;

            foreach (var configName in configNames)
            {
                if (ConfigLoader.CompileConfigToBytes(configName))
                {
                    successCount++;
                    GD.Print($"[ConfigCompiler] ✓ Compiled: {configName}");
                }
                else
                {
                    failCount++;
                    GD.PrintErr($"[ConfigCompiler] ✗ Failed: {configName}");
                }
            }

            GD.Print("========================================");
            GD.Print($"[ConfigCompiler] Compilation complete!");
            GD.Print($"  Success: {successCount}");
            GD.Print($"  Failed: {failCount}");
            GD.Print("========================================");
        }

        public static void EnableCompiledMode()
        {
            ConfigLoader.UseCompiledConfig = true;
            GD.Print("[ConfigCompiler] Switched to compiled config mode");
        }

        public static void DisableCompiledMode()
        {
            ConfigLoader.UseCompiledConfig = false;
            GD.Print("[ConfigCompiler] Switched to JSON config mode");
        }

        public static void TestConfigLoading()
        {
            GD.Print("========================================");
            GD.Print("[ConfigCompiler] Testing config loading...");
            GD.Print("========================================");

            TestConfig<CardConfigData>("cards");
            TestConfig<CharacterConfigData>("characters");
            TestConfig<EnemyConfigData>("enemies");
            TestConfig<RelicConfigData>("relics");
            TestConfig<PotionConfigData>("potions");
            TestConfig<EventConfigData>("events");
            TestConfig<AudioConfigData>("audio");
            TestConfig<EffectConfigData>("effects");

            GD.Print("========================================");
            GD.Print("[ConfigCompiler] All tests completed!");
            GD.Print("========================================");
        }

        private static void TestConfig<T>(string configName) where T : class
        {
            var config = ConfigLoader.LoadConfig<T>(configName);
            if (config != null)
            {
                GD.Print($"[ConfigCompiler] ✓ {configName}: Loaded successfully");
            }
            else
            {
                GD.PrintErr($"[ConfigCompiler] ✗ {configName}: Failed to load");
            }
        }
    }
}
