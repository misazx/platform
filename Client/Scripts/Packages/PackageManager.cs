using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Godot;
using RoguelikeGame.Core;

namespace RoguelikeGame.Packages
{
	public partial class PackageManager : SingletonBase<PackageManager>
	{
		private const string REGISTRY_URL = "http://localhost:8080/registry.json";
		private const string PACKAGES_DIR = "user://packages/";
		private const string REGISTRY_FILE = "user://package_registry.json";
		private const string STATE_FILE = "user://package_states.json";

		private Dictionary<string, PackageData> _availablePackages = new();
		private Dictionary<string, PackageInstallState> _installedPackages = new();
		private Dictionary<string, PackageConfig> _packageConfigs = new();
		private PackageRegistry _registry;
		private System.Net.Http.HttpClient _httpClient;

		[Signal]
		public delegate void PackageListUpdatedEventHandler();

		[Signal]
		public delegate void PackageDownloadProgressEventHandler(string packageId, float progress);

		[Signal]
		public delegate void PackageDownloadCompletedEventHandler(string packageId, bool success);

		[Signal]
		public delegate void PackageInstalledEventHandler(string packageId);

		[Signal]
		public delegate void PackageUninstalledEventHandler(string packageId);

		[Signal]
		public delegate void PackageErrorEventHandler(string packageId, string error);

		[Signal]
		public delegate void PackageConfigLoadedEventHandler(string packageId);

		[Signal]
		public delegate void PackageConfigChangedEventHandler(string packageId);

		public IReadOnlyDictionary<string, PackageData> AvailablePackages => _availablePackages;
		public IReadOnlyDictionary<string, PackageInstallState> InstalledPackages => _installedPackages;
		public IReadOnlyDictionary<string, PackageConfig> PackageConfigs => _packageConfigs;
		public PackageRegistry Registry => _registry;

		protected override void OnInitialize()
		{
			_httpClient = new System.Net.Http.HttpClient();
			_httpClient.Timeout = TimeSpan.FromMinutes(30);

			InitializePackageManager();
		}

		private async void InitializePackageManager()
		{
			GD.Print("[PackageManager] Initializing package manager...");

			EnsureDirectoriesExist();
			LoadLocalStates();
			await LoadRegistryAsync();
			RegisterBuiltinPackages();

			EmitSignal(SignalName.PackageListUpdated);
			GD.Print($"[PackageManager] Initialized with {_availablePackages.Count} available packages");
		}

		private void EnsureDirectoriesExist()
		{
			var dir = DirAccess.Open("user://");
			if (!dir.DirExists("packages"))
			{
				dir.MakeDir("packages");
			}
		}

		private void RegisterBuiltinPackages()
		{
			const string builtinConfigPath = "res://Config/Data/package_registry.json";
			
			if (!Godot.FileAccess.FileExists(builtinConfigPath))
			{
				GD.PushWarning("[PackageManager] No builtin package registry found, using fallback");
				_RegisterFallbackBaseGame();
				return;
			}

			try
			{
				using var file = Godot.FileAccess.Open(builtinConfigPath, Godot.FileAccess.ModeFlags.Read);
				var jsonText = file.GetAsText();
				var parsed = Json.ParseString(jsonText).As<Godot.Collections.Dictionary>();
				if (parsed == null)
				{
					_RegisterFallbackBaseGame();
					return;
				}

				if (!parsed.ContainsKey("packages"))
				{
					_RegisterFallbackBaseGame();
					return;
				}

				var packagesArray = parsed["packages"].AsGodotArray<Godot.Collections.Dictionary>();
				foreach (var pkgDict in packagesArray)
				{
					var isBuiltin = pkgDict.GetValueOrDefault("isFree", Variant.From(true)).AsBool();
					var pkgType = pkgDict.GetValueOrDefault("type", "").AsString();
					
					if (!isBuiltin && pkgType != "base_game")
						continue;

					var package = ParsePackageData(pkgDict);
					if (package == null || string.IsNullOrEmpty(package.Id))
						continue;

					_availablePackages[package.Id] = package;

					if (!_installedPackages.ContainsKey(package.Id))
					{
						_installedPackages[package.Id] = new PackageInstallState
						{
							PackageId = package.Id,
							Status = PackageStatus.Installed,
							InstalledVersion = package.Version,
							InstalledPath = "res://",
							InstallDate = DateTime.Now
						};
					}

					GD.Print($"[PackageManager] Registered builtin package: {package.Id} ({package.Name})");
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[PackageManager] Error loading builtin packages: {ex.Message}");
				_RegisterFallbackBaseGame();
			}
		}

		private void _RegisterFallbackBaseGame()
		{
			var baseGame = new PackageData
			{
				Id = "base_game",
				Name = "杀戮尖塔复刻版",
				Description = "经典Roguelike卡牌游戏体验，包含完整的主线剧情和核心玩法",
				Version = "1.0.0",
				Type = PackageType.BaseGame,
				Author = "Development Team",
				IconPath = "res://GameModes/base_game/Resources/Icons/Relics/burningblood.png",
				ThumbnailPath = "res://GameModes/base_game/Resources/Images/Backgrounds/glory.png",
				IsFree = true,
				RequiredBaseVersion = "1.0.0",
				Tags = new List<string> { "roguelike", "card", "strategy", "official" },
				Features = new List<string>
				{
					"经典卡牌战斗系统",
					"随机地图生成",
					"多角色选择",
					"圣物收集系统",
					"药水与事件"
				},
				EntryScene = "res://GameModes/base_game/Scenes/CombatScene.tscn",
				ConfigFile = "res://GameModes/base_game/Config/Data/cards.json",
				ReleaseDate = DateTime.Now,
				Rating = 4.8,
				DownloadCount = 10000
			};

			_availablePackages["base_game"] = baseGame;

			if (!_installedPackages.ContainsKey("base_game"))
			{
				_installedPackages["base_game"] = new PackageInstallState
				{
					PackageId = "base_game",
					Status = PackageStatus.Installed,
					InstalledVersion = baseGame.Version,
					InstalledPath = "res://",
					InstallDate = DateTime.Now
				};
			}
		}

		public async Task LoadRegistryAsync()
		{
			try
			{
				if (Godot.FileAccess.FileExists(REGISTRY_FILE))
				{
					using var file = Godot.FileAccess.Open(REGISTRY_FILE, Godot.FileAccess.ModeFlags.Read);
					var json = file.GetAsText();
					_registry = Json.Stringify(json).Contains("version") ? ParseRegistry(json) : new PackageRegistry();
					if (_registry == null) _registry = new PackageRegistry();
					PopulatePackagesFromRegistry();
				}
				else
				{
					_registry = new PackageRegistry();
					CreateDefaultCategories();
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[PackageManager] Error loading registry: {ex.Message}");
				_registry = new PackageRegistry();
				CreateDefaultCategories();
			}
		}

		private PackageRegistry ParseRegistry(string jsonString)
		{
			try
			{
				var parsed = Json.ParseString(jsonString).As<Godot.Collections.Dictionary>();
				if (parsed == null) return new PackageRegistry();

				var registry = new PackageRegistry
				{
					Version = parsed.GetValueOrDefault("version", "1.0.0").AsString(),
					Categories = new List<PackageCategory>(),
					Packages = new List<PackageData>(),
					FeaturedPackages = new List<string>()
				};

				if (parsed.ContainsKey("categories"))
				{
					var categoriesArray = parsed["categories"].AsGodotArray<Godot.Collections.Dictionary>();
					foreach (var catDict in categoriesArray)
					{
						registry.Categories.Add(new PackageCategory
						{
							Id = catDict.GetValueOrDefault("id", "").AsString(),
							Name = catDict.GetValueOrDefault("name", "").AsString(),
							Description = catDict.GetValueOrDefault("description", "").AsString(),
							Icon = catDict.GetValueOrDefault("icon", "").AsString(),
							PackageIds = ParseStringList(catDict, "packageIds")
						});
					}
				}

				if (parsed.ContainsKey("packages"))
				{
					var packagesArray = parsed["packages"].AsGodotArray<Godot.Collections.Dictionary>();
					foreach (var pkgDict in packagesArray)
					{
						registry.Packages.Add(ParsePackageData(pkgDict));
					}
				}

				if (parsed.ContainsKey("featuredPackages"))
				{
					registry.FeaturedPackages = ParseStringList(parsed, "featuredPackages");
				}

				return registry;
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[PackageManager] Error parsing registry: {ex.Message}");
				return new PackageRegistry();
			}
		}

		private List<string> ParseStringList(Godot.Collections.Dictionary dict, string key)
		{
			try
			{
				if (!dict.ContainsKey(key))
					return new List<string>();

				var arr = dict[key].AsGodotArray();
				var result = new List<string>();
				for (int i = 0; i < arr.Count; i++)
				{
					result.Add(arr[i].AsString());
				}
				return result;
			}
			catch
			{
				return new List<string>();
			}
		}

		private PackageData ParsePackageData(Godot.Collections.Dictionary pkgDict)
		{
			return new PackageData
			{
				Id = pkgDict.GetValueOrDefault("id", "").AsString(),
				Name = pkgDict.GetValueOrDefault("name", "").AsString(),
				Description = pkgDict.GetValueOrDefault("description", "").AsString(),
				Version = pkgDict.GetValueOrDefault("version", "1.0.0").AsString(),
				Author = pkgDict.GetValueOrDefault("author", "").AsString(),
				IconPath = pkgDict.GetValueOrDefault("iconPath", "").AsString(),
				ThumbnailPath = pkgDict.GetValueOrDefault("thumbnailPath", "").AsString(),
				DownloadUrl = pkgDict.GetValueOrDefault("downloadUrl", "").AsString(),
				FileSize = (long)pkgDict.GetValueOrDefault("fileSize", Variant.From(0L)).AsInt64(),
				RequiredBaseVersion = pkgDict.GetValueOrDefault("requiredBaseVersion", "1.0.0").AsString(),
				IsFree = pkgDict.GetValueOrDefault("isFree", Variant.From(true)).AsBool(),
				Price = (decimal)pkgDict.GetValueOrDefault("price", Variant.From(0.0)).AsDouble(),
				Rating = pkgDict.GetValueOrDefault("rating", Variant.From(0.0)).AsDouble(),
				DownloadCount = (int)ConvertToLong(pkgDict.GetValueOrDefault("downloadCount", Variant.From(0))),
				EntryScene = pkgDict.GetValueOrDefault("entryScene", "").AsString(),
				ConfigFile = pkgDict.GetValueOrDefault("configFile", "").AsString(),
				SupportsMultiplayer = ConvertToBool(pkgDict.GetValueOrDefault("supportsMultiplayer", Variant.From(false))),
				MaxPlayers = ConvertToInt(pkgDict.GetValueOrDefault("maxPlayers", Variant.From(4))),
				HasLeaderboard = ConvertToBool(pkgDict.GetValueOrDefault("hasLeaderboard", Variant.From(true))),
				LeaderboardType = pkgDict.GetValueOrDefault("leaderboardType", "score").AsString(),
				Tags = ParseStringList(pkgDict, "tags"),
				Features = ParseStringList(pkgDict, "features"),
				Dependencies = ParseStringList(pkgDict, "dependencies")
			};
		}

		private void CreateDefaultCategories()
		{
			_registry.Categories = new List<PackageCategory>
			{
				new PackageCategory
				{
					Id = "official",
					Name = "官方内容",
					Description = "开发团队制作的官方扩展包",
					Icon = "🎮",
					PackageIds = new List<string> { "base_game" }
				},
				new PackageCategory
				{
					Id = "community",
					Name = "社区创作",
					Description = "社区玩家创作的自定义内容",
					Icon = "👥",
					PackageIds = new List<string>()
				},
				new PackageCategory
				{
					Id = "dlc",
					Name = "DLC扩展",
					Description = "大型付费扩展内容",
					Icon = "💎",
					PackageIds = new List<string>()
				}
			};
		}

		private void PopulatePackagesFromRegistry()
		{
			if (_registry?.Packages == null) return;
			
			foreach (var package in _registry.Packages)
			{
				if (package != null && !string.IsNullOrEmpty(package.Id))
				{
					_availablePackages[package.Id] = package;
				}
			}
		}

		public async Task RefreshPackageListAsync()
		{
			try
			{
				var response = await _httpClient.GetAsync(REGISTRY_URL);
				response.EnsureSuccessStatusCode();

				var content = await response.Content.ReadAsStringAsync();
				
				using var file = Godot.FileAccess.Open(REGISTRY_FILE, Godot.FileAccess.ModeFlags.Write);
				file.StoreString(content);
				file.Close();

				await LoadRegistryAsync();

				EmitSignal(SignalName.PackageListUpdated);
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[PackageManager] Error refreshing package list: {ex.Message}");
				EmitSignal(SignalName.PackageError, "", $"刷新失败: {ex.Message}");
			}
		}

		public async Task DownloadPackageAsync(string packageId)
		{
			if (!_availablePackages.TryGetValue(packageId, out var package))
			{
				EmitSignal(SignalName.PackageError, packageId, "包不存在");
				return;
			}

			var state = GetOrCreateState(packageId);
			state.Status = PackageStatus.Downloading;
			state.DownloadProgress = 0f;
			SaveStates();

			try
			{
				var packagePath = Path.Combine(ProjectSettings.GlobalizePath(PACKAGES_DIR).TrimEnd('/'), $"{packageId}.zip");
				using var response = await _httpClient.GetAsync(package.DownloadUrl, HttpCompletionOption.ResponseHeadersRead);
				response.EnsureSuccessStatusCode();

				var totalBytes = response.Content.Headers.ContentLength ?? 0;
				var downloadedBytes = 0L;

				using var fs = System.IO.File.Create(packagePath);
				using var stream = await response.Content.ReadAsStreamAsync();
				var buffer = new byte[8192];
				int bytesRead;

				while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
				{
					await fs.WriteAsync(buffer, 0, bytesRead);
					downloadedBytes += bytesRead;

					if (totalBytes > 0)
					{
						state.DownloadProgress = (float)downloadedBytes / totalBytes;
						EmitSignal(SignalName.PackageDownloadProgress, packageId, state.DownloadProgress);
					}

					await Task.Delay(10);
				}

				state.Status = PackageStatus.Downloaded;
				SaveStates();
				EmitSignal(SignalName.PackageDownloadCompleted, packageId, true);

				await InstallPackageAsync(packageId);
			}
			catch (Exception ex)
			{
				state.Status = PackageStatus.Error;
				state.ErrorMessage = ex.Message;
				SaveStates();
				EmitSignal(SignalName.PackageError, packageId, $"下载失败: {ex.Message}");
				EmitSignal(SignalName.PackageDownloadCompleted, packageId, false);
			}
		}

		public async Task InstallPackageAsync(string packageId)
		{
			var state = GetOrCreateState(packageId);
			state.Status = PackageStatus.Installing;
			SaveStates();

			try
			{
				var packagesDir = ProjectSettings.GlobalizePath(PACKAGES_DIR).TrimEnd('/');
				var installPath = Path.Combine(packagesDir, packageId);
				var zipPath = Path.Combine(packagesDir, $"{packageId}.zip");

				if (!System.IO.Directory.Exists(installPath))
				{
					System.IO.Directory.CreateDirectory(installPath);
				}

				GD.Print($"[PackageManager] Extracting {packageId}.zip to {installPath}...");
				
				await Task.Run(() => ExtractZipFile(zipPath, installPath));

				GD.Print($"[PackageManager] Extraction complete for {packageId}");

				state.Status = PackageStatus.Installed;
				state.InstalledVersion = _availablePackages[packageId]?.Version;
				state.InstalledPath = installPath;
				state.InstallDate = DateTime.Now;
				SaveStates();

				EmitSignal(SignalName.PackageInstalled, packageId);

				LoadPackageConfig(packageId, installPath);

				var extension = LoadPackageExtension(packageId);
				extension?.OnInstalled();
			}
			catch (Exception ex)
			{
				state.Status = PackageStatus.Error;
				state.ErrorMessage = ex.Message;
				SaveStates();
				EmitSignal(SignalName.PackageError, packageId, $"安装失败: {ex.Message}");
			}
		}

		private void ExtractZipFile(string zipFilePath, string destinationPath)
		{
			if (!System.IO.File.Exists(zipFilePath))
			{
				throw new System.IO.FileNotFoundException($"ZIP file not found: {zipFilePath}");
			}

			using (var archive = System.IO.Compression.ZipFile.OpenRead(zipFilePath))
			{
				int totalEntries = archive.Entries.Count;
				int currentEntry = 0;

				foreach (var entry in archive.Entries)
				{
					currentEntry++;
					
					if (currentEntry % 100 == 0 || currentEntry == totalEntries)
					{
						float progress = (float)currentEntry / totalEntries * 100;
						GD.Print($"[PackageManager] Extracting... {progress:F0}% ({currentEntry}/{totalEntries})");
					}

					var destinationEntryPath = Path.Combine(destinationPath, entry.FullName);

					if (entry.FullName.EndsWith("/"))
					{
						if (!System.IO.Directory.Exists(destinationEntryPath))
						{
							System.IO.Directory.CreateDirectory(destinationEntryPath);
						}
					}
					else
					{
						var destinationDirectory = Path.GetDirectoryName(destinationEntryPath);
						if (!string.IsNullOrEmpty(destinationDirectory) && !System.IO.Directory.Exists(destinationDirectory))
						{
							System.IO.Directory.CreateDirectory(destinationDirectory);
						}

						using (var entryStream = entry.Open())
						using (var fileStream = System.IO.File.Create(destinationEntryPath))
						{
							entryStream.CopyTo(fileStream);
						}
					}
				}
			}

			GD.Print($"[PackageManager] ✅ Successfully extracted to {destinationPath}");
		}

		private int ConvertToInt(Variant value)
		{
			try
			{
				return value.AsInt32();
			}
			catch
			{
				return 0;
			}
		}

		private long ConvertToLong(Variant value)
		{
			try
			{
				return value.AsInt64();
			}
			catch
			{
				return 0L;
			}
		}

		public void UninstallPackage(string packageId)
		{
			if (packageId == "base_game")
			{
				EmitSignal(SignalName.PackageError, packageId, "无法卸载基础游戏");
				return;
			}

			if (!_installedPackages.TryGetValue(packageId, out var state))
			{
				return;
			}

			try
			{
				var extension = LoadPackageExtension(packageId);
				extension?.OnUninstalling();

				if (!string.IsNullOrEmpty(state.InstalledPath) && System.IO.Directory.Exists(state.InstalledPath))
				{
					System.IO.Directory.Delete(state.InstalledPath, true);
				}

				var packagesDir = ProjectSettings.GlobalizePath(PACKAGES_DIR).TrimEnd('/');
				var zipPath = Path.Combine(packagesDir, $"{packageId}.zip");
				if (System.IO.File.Exists(zipPath))
				{
					System.IO.File.Delete(zipPath);
				}

				_installedPackages.Remove(packageId);
				SaveStates();

				EmitSignal(SignalName.PackageUninstalled, packageId);
			}
			catch (Exception ex)
			{
				EmitSignal(SignalName.PackageError, packageId, $"卸载失败: {ex.Message}");
			}
		}

		public bool IsPackageInstalled(string packageId)
		{
			return _installedPackages.TryGetValue(packageId, out var state) &&
			       state.Status == PackageStatus.Installed;
		}

		public bool CanLaunchPackage(string packageId)
		{
			if (!IsPackageInstalled(packageId)) return false;

			var package = GetPackage(packageId);
			if (package == null) return false;

			foreach (var dep in package.Dependencies)
			{
				if (!IsPackageInstalled(dep)) return false;
			}

			return true;
		}

		public void LaunchPackage(string packageId)
		{
			if (!CanLaunchPackage(packageId))
			{
				EmitSignal(SignalName.PackageError, packageId, "无法启动：依赖未满足或未安装");
				return;
			}

			var package = GetPackage(packageId);
			if (_installedPackages.ContainsKey(packageId))
			{
				var state = _installedPackages[packageId];
				state.LastPlayed = DateTime.Now;
				SaveStates();
			}

			var extension = LoadPackageExtension(packageId);
			extension?.OnLaunch();

			EventBus.Instance.EmitSignal("StartGameRequested", packageId, package.EntryScene);
		}

		public PackageData GetPackage(string packageId)
		{
			_availablePackages.TryGetValue(packageId, out var package);
			return package;
		}

		public PackageInstallState GetPackageState(string packageId)
		{
			_installedPackages.TryGetValue(packageId, out var state);
			return state;
		}

		public List<PackageData> GetPackagesByCategory(string categoryId)
		{
			var category = _registry.Categories?.FirstOrDefault(c => c.Id == categoryId);
			if (category == null) return new List<PackageData>();

			return category.PackageIds
				.Select(id => GetPackage(id))
				.Where(p => p != null)
				.ToList();
		}

		public List<PackageData> GetFeaturedPackages()
		{
			return _registry.FeaturedPackages?
				.Select(id => GetPackage(id))
				.Where(p => p != null)
				.ToList() ?? new List<PackageData>();
		}

		public List<PackageData> SearchPackages(string query)
		{
			var lowerQuery = query.ToLower();
			return _availablePackages.Values
				.Where(p =>
					p.Name.ToLower().Contains(lowerQuery) ||
					p.Description.ToLower().Contains(lowerQuery) ||
					p.Tags.Any(t => t.ToLower().Contains(lowerQuery)))
				.ToList();
		}

		private IPackageExtension LoadPackageExtension(string packageId)
		{
			var state = GetPackageState(packageId);
			if (state?.InstalledPath == null) return null;

			var assemblyPath = Path.Combine(state.InstalledPath, $"{packageId}.dll");
			if (!System.IO.File.Exists(assemblyPath)) return null;

			try
			{
				// TODO: 实现动态加载程序集
				// 使用 System.Reflection.Assembly.LoadFrom 加载扩展
				return null;
			}
			catch
			{
				return null;
			}
		}

		private PackageInstallState GetOrCreateState(string packageId)
		{
			if (!_installedPackages.TryGetValue(packageId, out var state))
			{
				state = new PackageInstallState { PackageId = packageId };
				_installedPackages[packageId] = state;
			}
			return state;
		}

		private void LoadLocalStates()
		{
			try
			{
				if (Godot.FileAccess.FileExists(STATE_FILE))
				{
					using var file = Godot.FileAccess.Open(STATE_FILE, Godot.FileAccess.ModeFlags.Read);
					var json = file.GetAsText();
					
					if (!string.IsNullOrWhiteSpace(json))
					{
						var parsed = Json.ParseString(json).As<Godot.Collections.Dictionary>();
						if (parsed != null)
						{
							_installedPackages = new Dictionary<string, PackageInstallState>();
							if (parsed.ContainsKey("states"))
							{
								var statesArray = parsed["states"].AsGodotArray<Godot.Collections.Dictionary>();
								foreach (var stateDict in statesArray)
								{
									var state = new PackageInstallState
									{
										PackageId = stateDict.GetValueOrDefault("packageId", "").AsString(),
										Status = (PackageStatus)ConvertToInt(stateDict.GetValueOrDefault("status", Variant.From(0))),
										InstalledVersion = stateDict.GetValueOrDefault("installedVersion", "").AsString(),
										InstalledPath = stateDict.GetValueOrDefault("installedPath", "").AsString(),
										ErrorMessage = stateDict.GetValueOrDefault("errorMessage", "").AsString(),
										DownloadProgress = (float)stateDict.GetValueOrDefault("downloadProgress", 0f).AsDouble()
									};
									
									if (!string.IsNullOrEmpty(state.PackageId))
									{
										_installedPackages[state.PackageId] = state;
									}
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[PackageManager] Error loading states: {ex.Message}");
			}
		}

		private void SaveStates()
		{
			try
			{
				var statesArray = new Godot.Collections.Array();
				foreach (var kvp in _installedPackages)
				{
					var stateDict = new Godot.Collections.Dictionary
					{
						{ "packageId", kvp.Value.PackageId },
						{ "status", (int)kvp.Value.Status },
						{ "installedVersion", kvp.Value.InstalledVersion ?? "" },
						{ "installedPath", kvp.Value.InstalledPath ?? "" },
						{ "errorMessage", kvp.Value.ErrorMessage ?? "" },
						{ "downloadProgress", kvp.Value.DownloadProgress }
					};
					statesArray.Add(stateDict);
				}

				var rootDict = new Godot.Collections.Dictionary
				{
					{ "states", statesArray }
				};

				var json = Json.Stringify(rootDict, "\t");
				using var file = Godot.FileAccess.Open(STATE_FILE, Godot.FileAccess.ModeFlags.Write);
				file.StoreString(json);
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[PackageManager] Error saving states: {ex.Message}");
			}
		}

		public PackageConfig GetPackageConfig(string packageId)
		{
			if (_packageConfigs.TryGetValue(packageId, out var config))
			{
				return config;
			}

			if (_installedPackages.TryGetValue(packageId, out var state) && 
			    state.Status == PackageStatus.Installed && 
			    !string.IsNullOrEmpty(state.InstalledPath))
			{
				return LoadPackageConfig(packageId, state.InstalledPath);
			}

			return null;
		}

		public void UpdatePackageConfig(string packageId, Action<PackageConfig> updateAction)
		{
			var config = GetPackageConfig(packageId);
			if (config == null)
			{
				GD.PrintErr($"[PackageManager] Cannot update config for {packageId}: not loaded");
				return;
			}

			updateAction(config);
			config.LastModified = DateTime.Now;

			SavePackageConfig(packageId, config);

			EmitSignal(SignalName.PackageConfigChanged, packageId);

			GD.Print($"[PackageManager] Config updated for {packageId}");
		}

		public T GetGameplaySetting<T>(string packageId, string settingName, T defaultValue = default)
		{
			var config = GetPackageConfig(packageId);
			if (config?.Gameplay == null) return defaultValue;

			var property = typeof(GameplayConfig).GetProperty(settingName);
			if (property != null)
			{
				var value = property.GetValue(config.Gameplay);
				return value != null ? (T)value : defaultValue;
			}

			return defaultValue;
		}

		public void SetGameplaySetting<T>(string packageId, string settingName, T value)
		{
			UpdatePackageConfig(packageId, config =>
			{
				var property = typeof(GameplayConfig).GetProperty(settingName);
				if (property != null && property.CanWrite)
				{
					property.SetValue(config.Gameplay, value);
				}
			});
		}

		private PackageConfig LoadPackageConfig(string packageId, string installPath)
		{
			try
			{
				var possibleConfigPaths = new[]
				{
					Path.Combine(installPath, "base_game_config.json"),
					Path.Combine(installPath, "config.json"),
					Path.Combine(installPath, "package_config.json"),
					Path.Combine(installPath, "manifest.json")
				};

				foreach (var configPath in possibleConfigPaths)
				{
					if (System.IO.File.Exists(configPath))
					{
						GD.Print($"[PackageManager] Loading config from: {configPath}");

						var jsonContent = System.IO.File.ReadAllText(configPath);
						var config = ParsePackageConfig(jsonContent);

						if (config != null)
						{
							config.PackageId = packageId;
							_packageConfigs[packageId] = config;

							EmitSignal(SignalName.PackageConfigLoaded, packageId);

							GD.Print($"[PackageManager] ✅ Config loaded for {packageId}");
							GD.Print($"   Display: {config.DisplayName}");
							GD.Print($"   Version: {config.Version}");
							GD.Print($"   Starting Gold: {config.Gameplay.StartingGold}");
							GD.Print($"   Max Health: {config.Gameplay.MaxHealth}");

							return config;
						}
					}
				}

				GD.PushWarning($"[PackageManager] No config file found for {packageId}, using defaults");

				var defaultConfig = CreateDefaultConfig(packageId);
				_packageConfigs[packageId] = defaultConfig;

				EmitSignal(SignalName.PackageConfigLoaded, packageId);

				return defaultConfig;
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[PackageManager] Error loading config for {packageId}: {ex.Message}");

				var fallbackConfig = CreateDefaultConfig(packageId);
				_packageConfigs[packageId] = fallbackConfig;

				return fallbackConfig;
			}
		}

		private PackageConfig ParsePackageConfig(string jsonString)
		{
			try
			{
				var parsed = Json.ParseString(jsonString).As<Godot.Collections.Dictionary>();
				if (parsed == null) return null;

				var config = new PackageConfig
				{
					PackageId = parsed.GetValueOrDefault("packageId", "").AsString(),
					Version = parsed.GetValueOrDefault("version", "1.0.0").AsString(),
					DisplayName = parsed.GetValueOrDefault("displayName", "").AsString(),
					Description = parsed.GetValueOrDefault("description", "").AsString(),
					Author = parsed.GetValueOrDefault("author", "").AsString(),
					EntryScene = parsed.GetValueOrDefault("entryScene", "").AsString(),
					ConfigFile = parsed.GetValueOrDefault("configFile", "").AsString()
				};

				if (parsed.ContainsKey("gameplay"))
				{
					config.Gameplay = ParseGameplayConfig((Godot.Collections.Dictionary)parsed["gameplay"]);
				}

				if (parsed.ContainsKey("difficulty"))
				{
					config.Difficulty = ParseDifficultyConfig((Godot.Collections.Dictionary)parsed["difficulty"]);
				}

				if (parsed.ContainsKey("content"))
				{
					config.Content = ParseContentConfig((Godot.Collections.Dictionary)parsed["content"]);
				}

				if (parsed.ContainsKey("ui"))
				{
					config.UI = ParseUIConfig((Godot.Collections.Dictionary)parsed["ui"]);
				}

				if (parsed.ContainsKey("audio"))
				{
					config.Audio = ParseAudioConfig((Godot.Collections.Dictionary)parsed["audio"]);
				}

				if (parsed.ContainsKey("customSettings"))
				{
					var customDict = (Godot.Collections.Dictionary)parsed["customSettings"];
					foreach (var key in customDict.Keys)
					{
						config.CustomSettings[key.AsString()] = customDict[key];
					}
				}

				return config;
			}
			catch
			{
				return null;
			}
		}

		private GameplayConfig ParseGameplayConfig(Godot.Collections.Dictionary dict)
		{
			return new GameplayConfig
			{
				StartingGold = ConvertToInt(dict.GetValueOrDefault("startingGold", Variant.From(99))),
				MaxHealth = ConvertToInt(dict.GetValueOrDefault("maxHealth", Variant.From(80))),
				DrawCardsPerTurn = ConvertToInt(dict.GetValueOrDefault("drawCardsPerTurn", Variant.From(5))),
				EnergyPerTurn = ConvertToInt(dict.GetValueOrDefault("energyPerTurn", Variant.From(3))),
				MaxHandSize = ConvertToInt(dict.GetValueOrDefault("maxHandSize", Variant.From(10))),
				PotionSlots = ConvertToInt(dict.GetValueOrDefault("potionSlots", Variant.From(3))),
				RelicSlots = ConvertToInt(dict.GetValueOrDefault("relicSlots", Variant.From(8))),
				CardRewardCount = ConvertToInt(dict.GetValueOrDefault("cardRewardCount", Variant.From(3))),
				EnablePermanentUpgrades = ConvertToBool(dict.GetValueOrDefault("enablePermanentUpgrades", Variant.From(true))),
				EnableEvents = ConvertToBool(dict.GetValueOrDefault("enableEvents", Variant.From(true))),
				EnableShops = ConvertToBool(dict.GetValueOrDefault("enableShops", Variant.From(true))),
				EnableRestSites = ConvertToBool(dict.GetValueOrDefault("enableRestSites", Variant.From(true))),
				EnableEliteEncounters = ConvertToBool(dict.GetValueOrDefault("enableEliteEncounters", Variant.From(true))),
				EnableBossRush = ConvertToBool(dict.GetValueOrDefault("enableBossRush", Variant.From(false))),
				EnableDailyChallenge = ConvertToBool(dict.GetValueOrDefault("enableDailyChallenge", Variant.From(false))),
				EnableAscensionMode = ConvertToBool(dict.GetValueOrDefault("enableAscensionMode", Variant.From(true))),
				MaxAscensionLevel = ConvertToInt(dict.GetValueOrDefault("maxAscensionLevel", Variant.From(20)))
			};
		}

		private DifficultyConfig ParseDifficultyConfig(Godot.Collections.Dictionary dict)
		{
			return new DifficultyConfig
			{
				DefaultDifficulty = dict.GetValueOrDefault("defaultDifficulty", "normal").AsString(),
				AvailableDifficulties = ParseStringList(dict, "availableDifficulties"),
				EnemyScaling = ParseFloatDict(dict, "enemyScaling"),
				GoldMultiplier = ParseFloatDict(dict, "goldMultiplier"),
				HpModifier = ParseIntDict(dict, "hpModifier")
			};
		}

		private ContentConfig ParseContentConfig(Godot.Collections.Dictionary dict)
		{
			return new ContentConfig
			{
				EnabledCharacters = ParseStringList(dict, "enabledCharacters"),
				DisabledCharacters = ParseStringList(dict, "disabledCharacters"),
				EnabledCardPools = ParseStringList(dict, "enabledCardPools"),
				CustomCardSets = ParseStringList(dict, "customCardSets"),
				EnabledRelicPools = ParseStringList(dict, "enabledRelicPools"),
				EnabledEventPools = ParseStringList(dict, "enabledEventPools"),
				MaxFloorCount = ConvertToInt(dict.GetValueOrDefault("maxFloorCount", Variant.From(55)))
			};
		}

		private UIConfig ParseUIConfig(Godot.Collections.Dictionary dict)
		{
			return new UIConfig
			{
				Theme = dict.GetValueOrDefault("theme", "dark").AsString(),
				Language = dict.GetValueOrDefault("language", "zh-CN").AsString(),
				ShowDamageNumbers = ConvertToBool(dict.GetValueOrDefault("showDamageNumbers", Variant.From(true))),
				ShowBlockNumbers = ConvertToBool(dict.GetValueOrDefault("showBlockNumbers", Variant.From(true))),
				ShowIntentIcons = ConvertToBool(dict.GetValueOrDefault("showIntentIcons", Variant.From(true))),
				AnimationSpeed = (float)dict.GetValueOrDefault("animationSpeed", Variant.From(1.0)).AsDouble(),
				CardFlipAnimation = ConvertToBool(dict.GetValueOrDefault("cardFlipAnimation", Variant.From(true))),
				ScreenShakeIntensity = (float)dict.GetValueOrDefault("screenShakeIntensity", Variant.From(0.5)).AsDouble(),
				ParticleEffects = ConvertToBool(dict.GetValueOrDefault("particleEffects", Variant.From(true))),
				AutoPauseOnFocusLost = ConvertToBool(dict.GetValueOrDefault("autoPauseOnFocusLost", Variant.From(true)))
			};
		}

		private AudioConfig ParseAudioConfig(Godot.Collections.Dictionary dict)
		{
			return new AudioConfig
			{
				MasterVolume = (float)dict.GetValueOrDefault("masterVolume", Variant.From(1.0)).AsDouble(),
				BgmVolume = (float)dict.GetValueOrDefault("bgmVolume", Variant.From(0.8)).AsDouble(),
				SfxVolume = (float)dict.GetValueOrDefault("sfxVolume", Variant.From(1.0)).AsDouble(),
				AmbientVolume = (float)dict.GetValueOrDefault("ambientVolume", Variant.From(0.6)).AsDouble(),
				VoiceVolume = (float)dict.GetValueOrDefault("voiceVolume", Variant.From(1.0)).AsDouble(),
				EnableDynamicMusic = ConvertToBool(dict.GetValueOrDefault("enableDynamicMusic", Variant.From(true))),
				CombatMusicIntensity = ConvertToBool(dict.GetValueOrDefault("combatMusicIntensity", Variant.From(true)))
			};
		}

		private Dictionary<string, float> ParseFloatDict(Godot.Collections.Dictionary parent, string key)
		{
			var result = new Dictionary<string, float>();
			if (!parent.ContainsKey(key)) return result;

			var dict = (Godot.Collections.Dictionary)parent[key];
			foreach (var k in dict.Keys)
			{
				result[k.AsString()] = (float)dict[k].AsDouble();
			}
			return result;
		}

		private Dictionary<string, int> ParseIntDict(Godot.Collections.Dictionary parent, string key)
		{
			var result = new Dictionary<string, int>();
			if (!parent.ContainsKey(key)) return result;

			var dict = (Godot.Collections.Dictionary)parent[key];
			foreach (var k in dict.Keys)
			{
				result[k.AsString()] = ConvertToInt(dict[k]);
			}
			return result;
		}

		private bool ConvertToBool(Variant value)
		{
			try
			{
				return value.AsBool();
			}
			catch
			{
				return true;
			}
		}

		private void SavePackageConfig(string packageId, PackageConfig config)
		{
			if (!_installedPackages.TryGetValue(packageId, out var state) || 
			    string.IsNullOrEmpty(state.InstalledPath))
			{
				GD.PrintErr($"[PackageManager] Cannot save config for {packageId}: not installed");
				return;
			}

			try
			{
				var configPath = Path.Combine(state.InstalledPath, "base_game_config.json");
				
				var rootDict = new Godot.Collections.Dictionary
				{
					{ "packageId", config.PackageId },
					{ "version", config.Version },
					{ "displayName", config.DisplayName },
					{ "description", config.Description },
					{ "author", config.Author },
					{ "entryScene", config.EntryScene },
					{ "configFile", config.ConfigFile },
					{ "gameplay", SerializeGameplayConfig(config.Gameplay) },
					{ "difficulty", SerializeDifficultyConfig(config.Difficulty) },
					{ "content", SerializeContentConfig(config.Content) },
					{ "ui", SerializeUIConfig(config.UI) },
					{ "audio", SerializeAudioConfig(config.Audio) },
					{ "customSettings", SerializeCustomSettings(config.CustomSettings) },
					{ "lastModified", DateTime.Now.ToString("o") }
				};

				var jsonString = Json.Stringify(rootDict, "\t");
				System.IO.File.WriteAllText(configPath, jsonString);

				GD.Print($"[PackageManager] Config saved to: {configPath}");
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[PackageManager] Error saving config for {packageId}: {ex.Message}");
			}
		}

		private Godot.Collections.Dictionary SerializeGameplayConfig(GameplayConfig gameplay)
		{
			return new Godot.Collections.Dictionary
			{
				{ "startingGold", gameplay.StartingGold },
				{ "maxHealth", gameplay.MaxHealth },
				{ "drawCardsPerTurn", gameplay.DrawCardsPerTurn },
				{ "energyPerTurn", gameplay.EnergyPerTurn },
				{ "maxHandSize", gameplay.MaxHandSize },
				{ "potionSlots", gameplay.PotionSlots },
				{ "relicSlots", gameplay.RelicSlots },
				{ "cardRewardCount", gameplay.CardRewardCount },
				{ "enablePermanentUpgrades", gameplay.EnablePermanentUpgrades },
				{ "enableEvents", gameplay.EnableEvents },
				{ "enableShops", gameplay.EnableShops },
				{ "enableRestSites", gameplay.EnableRestSites },
				{ "enableEliteEncounters", gameplay.EnableEliteEncounters },
				{ "enableBossRush", gameplay.EnableBossRush },
				{ "enableDailyChallenge", gameplay.EnableDailyChallenge },
				{ "enableAscensionMode", gameplay.EnableAscensionMode },
				{ "maxAscensionLevel", gameplay.MaxAscensionLevel }
			};
		}

		private Godot.Collections.Dictionary SerializeDifficultyConfig(DifficultyConfig difficulty)
		{
			return new Godot.Collections.Dictionary
			{
				{ "defaultDifficulty", difficulty.DefaultDifficulty },
				{ "availableDifficulties", ToGodotArray(difficulty.AvailableDifficulties) },
				{ "enemyScaling", ToGodotDict(difficulty.EnemyScaling) },
				{ "goldMultiplier", ToGodotDict(difficulty.GoldMultiplier) },
				{ "hpModifier", ToGodotDict(difficulty.HpModifier) }
			};
		}

		private Godot.Collections.Dictionary SerializeContentConfig(ContentConfig content)
		{
			return new Godot.Collections.Dictionary
			{
				{ "enabledCharacters", ToGodotArray(content.EnabledCharacters) },
				{ "disabledCharacters", ToGodotArray(content.DisabledCharacters) },
				{ "enabledCardPools", ToGodotArray(content.EnabledCardPools) },
				{ "customCardSets", ToGodotArray(content.CustomCardSets) },
				{ "enabledRelicPools", ToGodotArray(content.EnabledRelicPools) },
				{ "enabledEventPools", ToGodotArray(content.EnabledEventPools) },
				{ "maxFloorCount", content.MaxFloorCount }
			};
		}

		private Godot.Collections.Dictionary SerializeUIConfig(UIConfig ui)
		{
			return new Godot.Collections.Dictionary
			{
				{ "theme", ui.Theme },
				{ "language", ui.Language },
				{ "showDamageNumbers", ui.ShowDamageNumbers },
				{ "showBlockNumbers", ui.ShowBlockNumbers },
				{ "showIntentIcons", ui.ShowIntentIcons },
				{ "animationSpeed", ui.AnimationSpeed },
				{ "cardFlipAnimation", ui.CardFlipAnimation },
				{ "screenShakeIntensity", ui.ScreenShakeIntensity },
				{ "particleEffects", ui.ParticleEffects },
				{ "autoPauseOnFocusLost", ui.AutoPauseOnFocusLost }
			};
		}

		private Godot.Collections.Dictionary SerializeAudioConfig(AudioConfig audio)
		{
			return new Godot.Collections.Dictionary
			{
				{ "masterVolume", audio.MasterVolume },
				{ "bgmVolume", audio.BgmVolume },
				{ "sfxVolume", audio.SfxVolume },
				{ "ambientVolume", audio.AmbientVolume },
				{ "voiceVolume", audio.VoiceVolume },
				{ "enableDynamicMusic", audio.EnableDynamicMusic },
				{ "combatMusicIntensity", audio.CombatMusicIntensity }
			};
		}

		private Godot.Collections.Dictionary SerializeCustomSettings(Dictionary<string, Variant> settings)
		{
			var dict = new Godot.Collections.Dictionary();
			foreach (var kvp in settings)
			{
				dict[kvp.Key] = kvp.Value;
			}
			return dict;
		}

		private Godot.Collections.Array ToGodotArray(List<string> list)
		{
			var arr = new Godot.Collections.Array();
			foreach (var item in list)
			{
				arr.Add(item);
			}
			return arr;
		}

		private Godot.Collections.Dictionary ToGodotDict(Dictionary<string, float> dict)
		{
			var result = new Godot.Collections.Dictionary();
			foreach (var kvp in dict)
			{
				result[kvp.Key] = kvp.Value;
			}
			return result;
		}

		private Godot.Collections.Dictionary ToGodotDict(Dictionary<string, int> dict)
		{
			var result = new Godot.Collections.Dictionary();
			foreach (var kvp in dict)
			{
				result[kvp.Key] = kvp.Value;
			}
			return result;
		}

		private PackageConfig CreateDefaultConfig(string packageId)
		{
			return new PackageConfig
			{
				PackageId = packageId,
				DisplayName = _availablePackages.GetValueOrDefault(packageId)?.Name ?? "Unknown Package",
				Gameplay = new GameplayConfig(),
				Difficulty = new DifficultyConfig(),
				Content = new ContentConfig(),
				UI = new UIConfig(),
				Audio = new AudioConfig()
			};
		}

		public override void _ExitTree()
		{
			_httpClient?.Dispose();
			base._ExitTree();
		}
	}
}
