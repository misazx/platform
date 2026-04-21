# Graph Report - .  (2026-04-21)

## Corpus Check
- 158 files · ~957,696 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 2006 nodes · 3203 edges · 114 communities detected
- Extraction: 99% EXTRACTED · 1% INFERRED · 0% AMBIGUOUS · INFERRED: 24 edges (avg confidence: 0.5)
- Token cost: 0 input · 0 output

## God Nodes (most connected - your core abstractions)
1. `ArtGenerator` - 75 edges
2. `ResourceGenerator` - 63 edges
3. `Main` - 58 edges
4. `PackageManager` - 57 edges
5. `GameHubClient` - 36 edges
6. `RoomPanel` - 35 edges
7. `EnhancedMainMenu` - 32 edges
8. `MainWindow` - 30 edges
9. `GameSessionManager` - 27 edges
10. `BuildTool` - 26 edges

## Surprising Connections (you probably didn't know these)
- `BasePlatform` --uses--> `BuildConfig`  [INFERRED]
  Client/Tools/packaging/platforms/base.py → Client/Tools/packaging/core/config.py
- `BasePlatform` --uses--> `EnvironmentDetector`  [INFERRED]
  Client/Tools/packaging/platforms/base.py → Client/Tools/packaging/core/environment.py
- `BuildEngine` --uses--> `BuildConfig`  [INFERRED]
  Client/Tools/packaging/core/engine.py → Client/Tools/packaging/core/config.py
- `BuildEngine` --uses--> `EnvironmentDetector`  [INFERRED]
  Client/Tools/packaging/core/engine.py → Client/Tools/packaging/core/environment.py
- `SignalBridge` --uses--> `BuildEngine`  [INFERRED]
  Client/Tools/packaging/gui/main_window.py → Client/Tools/packaging/core/engine.py

## Communities

### Community 0 - "Community 0"
Cohesion: 0.02
Nodes (74): ConfigCompiler, RoguelikeGame.Editor, Control, GameInitializer, RoguelikeGame.Core, AchievementManager, AchievementPanel, AchievementSystem (+66 more)

### Community 1 - "Community 1"
Cohesion: 0.03
Nodes (13): ABC, AndroidPlatform, BasePlatform, BasePlatform, BuildEngine, IOSPlatform, shutil_which(), MacOSPlatform (+5 more)

### Community 2 - "Community 2"
Cohesion: 0.08
Nodes (2): ArtGenerator, RoguelikeGame.Core

### Community 3 - "Community 3"
Cohesion: 0.04
Nodes (18): EventBus, GameEvents, RoguelikeGame.Core, GameManager, RoguelikeGame.Core, RunData, IUIScreenController, IUIScreenController (+10 more)

### Community 4 - "Community 4"
Cohesion: 0.09
Nodes (2): ResourceGenerator, RoguelikeGame.Core

### Community 5 - "Community 5"
Cohesion: 0.05
Nodes (13): build_number(), BuildConfig, bundle_id(), dotnet_root(), godot_path(), project_name(), version(), EnvCheckResult (+5 more)

### Community 6 - "Community 6"
Cohesion: 0.03
Nodes (26): AchievementController, AchievementItem, RoguelikeGame.Server.Controllers, SaveController, SyncAchievementsRequest, UploadSaveRequest, AuthController, LoginRequest (+18 more)

### Community 7 - "Community 7"
Cohesion: 0.07
Nodes (2): Main, RoguelikeGame

### Community 8 - "Community 8"
Cohesion: 0.08
Nodes (2): PackageManager, RoguelikeGame.Packages

### Community 9 - "Community 9"
Cohesion: 0.05
Nodes (17): BackgroundService, BehaviorTree, RoguelikeGame.Shared.BehaviorTree, BotGameService, IBotGameService, RoguelikeGame.Server.Services, RoomBotContext, BotActions (+9 more)

### Community 10 - "Community 10"
Cohesion: 0.1
Nodes (14): CheckpointData, FragmentData, GoalData, LightShadowActions, LightShadowBBKeys, LightShadowBotAI, LightShadowBotAIFactory, LightShadowEnemyData (+6 more)

### Community 11 - "Community 11"
Cohesion: 0.05
Nodes (13): BTAction, RoguelikeGame.Shared.BehaviorTree, BTCondition, RoguelikeGame.Shared.BehaviorTree, BTDecorator, RoguelikeGame.Shared.BehaviorTree, BTNode, BTNode (+5 more)

### Community 12 - "Community 12"
Cohesion: 0.06
Nodes (4): BaseGameExtension, IPackageExtension, PackageExtensionBase, RoguelikeGame.Packages

### Community 13 - "Community 13"
Cohesion: 0.07
Nodes (2): GameHubClient, RoguelikeGame.Network.Realtime

### Community 14 - "Community 14"
Cohesion: 0.11
Nodes (2): RoguelikeGame.UI.Panels, RoomPanel

### Community 15 - "Community 15"
Cohesion: 0.06
Nodes (5): GameHub, RoguelikeGame.Server.Hubs, Hub, LobbyHub, RoguelikeGame.Server.Hubs

### Community 16 - "Community 16"
Cohesion: 0.1
Nodes (2): EnhancedMainMenu, RoguelikeGame.UI

### Community 17 - "Community 17"
Cohesion: 0.09
Nodes (5): CardAction, GameSessionManager, GameState, PlayerState, RoguelikeGame.Network.Session

### Community 18 - "Community 18"
Cohesion: 0.13
Nodes (4): BuildTool, main(), 构建包含 .NET 环境变量的环境 - 基于已注入的 os.environ, 线程安全的日志方法 - 使用 root.after() 在主线程更新GUI

### Community 19 - "Community 19"
Cohesion: 0.11
Nodes (4): AuthResult, AuthSystem, RoguelikeGame.Network.Auth, UserInfo

### Community 20 - "Community 20"
Cohesion: 0.16
Nodes (6): ArtAssetManager, AssetMapping, AssetSource, main(), 添加自定义美术资源（统一工作流入口）                  Args:             source_path: 源文件路径（可以是本地文件, 从目录批量导入资源                  Args:             source_dir: 源目录             target_

### Community 21 - "Community 21"
Cohesion: 0.13
Nodes (5): PlayerInfo, RoguelikeGame.Network.Rooms, RoomInfo, RoomManager, RoomResult

### Community 22 - "Community 22"
Cohesion: 0.08
Nodes (3): IRoomService, RoguelikeGame.Server.Services, RoomService

### Community 23 - "Community 23"
Cohesion: 0.14
Nodes (2): LobbyPanel, RoguelikeGame.UI.Panels

### Community 24 - "Community 24"
Cohesion: 0.15
Nodes (24): call_doubao_api(), _draw_boss_enemy(), _draw_concert_bg(), _draw_effect(), _draw_forest_bg(), _draw_fragment(), _draw_game_icon(), _draw_heart() (+16 more)

### Community 25 - "Community 25"
Cohesion: 0.14
Nodes (2): PackageStoreUI, RoguelikeGame.UI

### Community 26 - "Community 26"
Cohesion: 0.2
Nodes (15): call_seedream_api(), download_image(), generate_single_resource(), GenStats, load_document(), main(), ModelRotator, preview_document() (+7 more)

### Community 27 - "Community 27"
Cohesion: 0.14
Nodes (3): IUIScreen, RoguelikeGame.UI, UIManager

### Community 28 - "Community 28"
Cohesion: 0.09
Nodes (7): ENetConnectionAdapter, RoguelikeGame.Network.Core, IConnectionAdapter, RoguelikeGame.Network.Core, IDisposable, RoguelikeGame.Network.Core, WebSocketConnectionAdapter

### Community 29 - "Community 29"
Cohesion: 0.16
Nodes (5): AutoAssetDownloader, main(), 下载文件，按优先级尝试多种方法：         1. curl (最可靠)         2. wget         3. urllib (带SSL修复, 尝试使用 urllib 下载（带SSL证书问题修复）, ReliableDownloader

### Community 30 - "Community 30"
Cohesion: 0.14
Nodes (3): LANDiscoveryService, LANHostInfo, RoguelikeGame.Network.Discovery

### Community 31 - "Community 31"
Cohesion: 0.21
Nodes (2): KenneyStyleGenerator, main()

### Community 32 - "Community 32"
Cohesion: 0.19
Nodes (3): CDNHandler, create_test_directory_structure(), main()

### Community 33 - "Community 33"
Cohesion: 0.15
Nodes (2): MultiProtocolManager, RoguelikeGame.Network

### Community 34 - "Community 34"
Cohesion: 0.14
Nodes (2): MainMenuNetworkIntegrator, RoguelikeGame.UI

### Community 35 - "Community 35"
Cohesion: 0.21
Nodes (2): GuaranteedAssetGenerator, main()

### Community 36 - "Community 36"
Cohesion: 0.18
Nodes (2): NetworkManager, RoguelikeGame.Network

### Community 37 - "Community 37"
Cohesion: 0.23
Nodes (5): AssetFixer, main(), 将 Kenney 资源映射到配置文件需要的路径, 替换角色立绘为更好的版本或使用 Kenney 资源, 使用 Kenney 风格重新生成缺失的关键资源

### Community 38 - "Community 38"
Cohesion: 0.44
Nodes (8): generate_backgrounds(), generate_character_portraits(), generate_enemy_images(), generate_potion_images(), generate_relic_images(), generate_ui_icons(), ImageGenerator, main()

### Community 39 - "Community 39"
Cohesion: 0.27
Nodes (4): KenneyOfficialDownloader, main(), 从 Kenney.nl 页面提取真实的下载链接                  关键发现：Kenney 的下载链接格式为：         https://k, 处理单个资源包：提取URL → 下载 → 解压整合

### Community 40 - "Community 40"
Cohesion: 0.16
Nodes (3): AuthService, IAuthService, RoguelikeGame.Server.Services

### Community 41 - "Community 41"
Cohesion: 0.29
Nodes (14): download_and_integrate_kenney_pack(), download_file(), extract_zip(), find_png_files(), generate_integration_report(), init_directories(), integrate_assets(), _is_placeholder() (+6 more)

### Community 42 - "Community 42"
Cohesion: 0.24
Nodes (3): NetworkPacket, PacketSerializer, RoguelikeGame.Network.Core

### Community 43 - "Community 43"
Cohesion: 0.15
Nodes (3): ILeaderboardService, LeaderboardService, RoguelikeGame.Server.Services

### Community 44 - "Community 44"
Cohesion: 0.27
Nodes (2): BuildTool, main()

### Community 45 - "Community 45"
Cohesion: 0.34
Nodes (3): main(), 使用多重 URL 故障转移的可靠下载器         按顺序尝试每个 URL，直到成功或全部失败, UltimateDownloader

### Community 46 - "Community 46"
Cohesion: 0.27
Nodes (2): main(), OneClickDownloader

### Community 47 - "Community 47"
Cohesion: 0.26
Nodes (3): AIAssetMatcher, main(), 清理 .import 缓存（不删除 .godot 目录，让 Godot 自行管理）

### Community 48 - "Community 48"
Cohesion: 0.15
Nodes (4): PackageExtensionBase, FrostExpansion, RoguelikeGame.Packages.Samples, ShadowRealmExtension

### Community 49 - "Community 49"
Cohesion: 0.21
Nodes (3): BotInstance, BotManager, RoguelikeGame.Shared.Bots

### Community 50 - "Community 50"
Cohesion: 0.27
Nodes (3): main(), 将 Godot Web 导出转换为微信小游戏          Args:             input_dir: Godot Web 导出的目录路径, WeChatConverter

### Community 51 - "Community 51"
Cohesion: 0.29
Nodes (12): batch_import(), categorize_by_filename(), categorize_file(), ensure_directories(), find_image_files(), generate_config_suggestions(), _get_category_usage(), _get_config_field_for_category() (+4 more)

### Community 52 - "Community 52"
Cohesion: 0.27
Nodes (2): LeaderboardPanel, RoguelikeGame.UI.Panels

### Community 53 - "Community 53"
Cohesion: 0.23
Nodes (2): RandomGenerator, RoguelikeGame.Core

### Community 54 - "Community 54"
Cohesion: 0.15
Nodes (2): OfflineModeManager, RoguelikeGame.Network

### Community 55 - "Community 55"
Cohesion: 0.19
Nodes (2): ConnectionManager, RoguelikeGame.Network.Core

### Community 56 - "Community 56"
Cohesion: 0.24
Nodes (3): FriendInfo, FriendManager, RoguelikeGame.Network.Friends

### Community 57 - "Community 57"
Cohesion: 0.2
Nodes (3): IMatchmakingService, MatchmakingService, RoguelikeGame.Server.Services

### Community 58 - "Community 58"
Cohesion: 0.32
Nodes (2): AudioGenerator, main()

### Community 59 - "Community 59"
Cohesion: 0.27
Nodes (2): LoginPanel, RoguelikeGame.UI.Panels

### Community 60 - "Community 60"
Cohesion: 0.2
Nodes (2): Blackboard, RoguelikeGame.Shared.BehaviorTree

### Community 61 - "Community 61"
Cohesion: 0.27
Nodes (2): MultiplayerPanel, RoguelikeGame.UI.Panels

### Community 62 - "Community 62"
Cohesion: 0.27
Nodes (2): MainMenu, RoguelikeGame.UI

### Community 63 - "Community 63"
Cohesion: 0.47
Nodes (8): apply_suggestions(), find_available_assets(), load_config(), main(), save_config(), suggest_card_updates(), suggest_character_updates(), suggest_enemy_updates()

### Community 64 - "Community 64"
Cohesion: 0.22
Nodes (8): ActConfig, AudioConfig, ContentConfig, DifficultyConfig, GameplayConfig, PackageConfig, RoguelikeGame.Packages, UIConfig

### Community 65 - "Community 65"
Cohesion: 0.29
Nodes (2): pollEvents(), updateStats()

### Community 66 - "Community 66"
Cohesion: 0.46
Nodes (7): clean_all_import_caches(), fix_filename_casing(), force_godot_reimport(), load_json(), main(), regenerate_all_assets(), verify_assets()

### Community 67 - "Community 67"
Cohesion: 0.36
Nodes (2): GameModeSelectPanel, RoguelikeGame.UI.Panels

### Community 68 - "Community 68"
Cohesion: 0.39
Nodes (2): ConnectionStatusIndicator, RoguelikeGame.UI.Panels

### Community 69 - "Community 69"
Cohesion: 0.57
Nodes (6): compute_hash(), create_package(), generate_hotfix_manifest(), generate_registry(), main(), should_exclude()

### Community 70 - "Community 70"
Cohesion: 0.29
Nodes (0): 

### Community 71 - "Community 71"
Cohesion: 0.52
Nodes (6): create_directory_structure(), create_readme(), execute_migration(), generate_path_mapping(), main(), plan_migration()

### Community 72 - "Community 72"
Cohesion: 0.33
Nodes (5): CreateRoomRequest, JoinRoomRequest, RoguelikeGame.Shared.Protocol, RoomInfo, RoomPlayerInfo

### Community 73 - "Community 73"
Cohesion: 0.33
Nodes (5): PackageCategory, PackageData, PackageInstallState, PackageRegistry, RoguelikeGame.Packages

### Community 74 - "Community 74"
Cohesion: 0.4
Nodes (3): ApplicationDbContext, RoguelikeGame.Server.Data, DbContext

### Community 75 - "Community 75"
Cohesion: 0.4
Nodes (4): AuthRequest, AuthResponse, RoguelikeGame.Shared.Protocol, UserInfo

### Community 76 - "Community 76"
Cohesion: 0.4
Nodes (4): GameAction, GameResult, GameStateUpdate, RoguelikeGame.Shared.Protocol

### Community 77 - "Community 77"
Cohesion: 0.5
Nodes (4): build_monster(), copy_and_resize(), map_all_resources(), 用 Monster Builder Pack 部件组装怪物

### Community 78 - "Community 78"
Cohesion: 0.4
Nodes (2): CardStyleConfig, RoguelikeGame.Core

### Community 79 - "Community 79"
Cohesion: 0.5
Nodes (3): AchievementEntry, RoguelikeGame.Server.Models, SaveEntry

### Community 80 - "Community 80"
Cohesion: 0.5
Nodes (3): RoguelikeGame.Server.Models, Room, RoomPlayer

### Community 81 - "Community 81"
Cohesion: 0.5
Nodes (2): BotProfile, RoguelikeGame.Shared.Bots

### Community 82 - "Community 82"
Cohesion: 0.5
Nodes (2): analyze_kenney_page(), 分析 Kenney.nl 页面，提取下载信息

### Community 83 - "Community 83"
Cohesion: 0.67
Nodes (2): find_tiles(), make_background()

### Community 84 - "Community 84"
Cohesion: 0.67
Nodes (2): LeaderboardEntry, RoguelikeGame.Server.Models

### Community 85 - "Community 85"
Cohesion: 0.67
Nodes (2): RoguelikeGame.Server.Models, User

### Community 86 - "Community 86"
Cohesion: 0.67
Nodes (2): Friendship, RoguelikeGame.Server.Models

### Community 87 - "Community 87"
Cohesion: 0.67
Nodes (2): BTContext, RoguelikeGame.Shared.BehaviorTree

### Community 88 - "Community 88"
Cohesion: 0.67
Nodes (0): 

### Community 89 - "Community 89"
Cohesion: 1.0
Nodes (0): 

### Community 90 - "Community 90"
Cohesion: 1.0
Nodes (0): 

### Community 91 - "Community 91"
Cohesion: 1.0
Nodes (0): 

### Community 92 - "Community 92"
Cohesion: 1.0
Nodes (0): 

### Community 93 - "Community 93"
Cohesion: 1.0
Nodes (0): 

### Community 94 - "Community 94"
Cohesion: 1.0
Nodes (0): 

### Community 95 - "Community 95"
Cohesion: 1.0
Nodes (0): 

### Community 96 - "Community 96"
Cohesion: 1.0
Nodes (0): 

### Community 97 - "Community 97"
Cohesion: 1.0
Nodes (0): 

### Community 98 - "Community 98"
Cohesion: 1.0
Nodes (0): 

### Community 99 - "Community 99"
Cohesion: 1.0
Nodes (0): 

### Community 100 - "Community 100"
Cohesion: 1.0
Nodes (0): 

### Community 101 - "Community 101"
Cohesion: 1.0
Nodes (0): 

### Community 102 - "Community 102"
Cohesion: 1.0
Nodes (0): 

### Community 103 - "Community 103"
Cohesion: 1.0
Nodes (0): 

### Community 104 - "Community 104"
Cohesion: 1.0
Nodes (0): 

### Community 105 - "Community 105"
Cohesion: 1.0
Nodes (0): 

### Community 106 - "Community 106"
Cohesion: 1.0
Nodes (0): 

### Community 107 - "Community 107"
Cohesion: 1.0
Nodes (0): 

### Community 108 - "Community 108"
Cohesion: 1.0
Nodes (0): 

### Community 109 - "Community 109"
Cohesion: 1.0
Nodes (0): 

### Community 110 - "Community 110"
Cohesion: 1.0
Nodes (0): 

### Community 111 - "Community 111"
Cohesion: 1.0
Nodes (0): 

### Community 112 - "Community 112"
Cohesion: 1.0
Nodes (0): 

### Community 113 - "Community 113"
Cohesion: 1.0
Nodes (0): 

## Knowledge Gaps
- **196 isolated node(s):** `RoguelikeGame.Server.Models`, `LeaderboardEntry`, `RoguelikeGame.Server.Models`, `AchievementEntry`, `SaveEntry` (+191 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **Thin community `Community 89`** (2 nodes): `generate_icon.py`, `create_icon()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 90`** (2 nodes): `extract_sprout_lands_ui.py`, `crop_and_save()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 91`** (2 nodes): `run.py`, `main()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 92`** (1 nodes): `emit_test.py`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 93`** (1 nodes): `quick_test.py`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 94`** (1 nodes): `validate_levels.py`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 95`** (1 nodes): `check_js2.py`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 96`** (1 nodes): `viz_standalone.js`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 97`** (1 nodes): `viz_check.js`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 98`** (1 nodes): `start_viz.py`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 99`** (1 nodes): `check_js3.py`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 100`** (1 nodes): `check_js.py`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 101`** (1 nodes): `Program.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 102`** (1 nodes): `RoguelikeGame.Server.MvcApplicationPartsAssemblyInfo.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 103`** (1 nodes): `RoguelikeGame.Server.AssemblyInfo.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 104`** (1 nodes): `RoguelikeGame.Server.GlobalUsings.g.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 105`** (1 nodes): `RoguelikeGame.Shared.GlobalUsings.g.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 106`** (1 nodes): `RoguelikeGame.Shared.AssemblyInfo.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 107`** (1 nodes): `map_ui_assets.py`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 108`** (1 nodes): `_diag2.py`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 109`** (1 nodes): `_diag.py`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 110`** (1 nodes): `download_more_packs.py`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 111`** (1 nodes): `generate_player_icon.py`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 112`** (1 nodes): `download_missing_packs.py`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 113`** (1 nodes): `__init__.py`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `SingletonBase` connect `Community 3` to `Community 0`?**
  _High betweenness centrality (0.104) - this node is a cross-community bridge._
- **Why does `ArtGenerator` connect `Community 2` to `Community 3`?**
  _High betweenness centrality (0.034) - this node is a cross-community bridge._
- **Why does `ResourceGenerator` connect `Community 4` to `Community 3`?**
  _High betweenness centrality (0.028) - this node is a cross-community bridge._
- **What connects `RoguelikeGame.Server.Models`, `LeaderboardEntry`, `RoguelikeGame.Server.Models` to the rest of the system?**
  _196 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Community 0` be split into smaller, more focused modules?**
  _Cohesion score 0.02 - nodes in this community are weakly interconnected._
- **Should `Community 1` be split into smaller, more focused modules?**
  _Cohesion score 0.03 - nodes in this community are weakly interconnected._
- **Should `Community 2` be split into smaller, more focused modules?**
  _Cohesion score 0.08 - nodes in this community are weakly interconnected._