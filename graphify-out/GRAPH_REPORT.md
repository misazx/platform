# Graph Report - .  (2026-04-13)

## Corpus Check
- 176 files · ~220,657 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 2525 nodes · 4022 edges · 135 communities detected
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS
- Token cost: 0 input · 0 output

## God Nodes (most connected - your core abstractions)
1. `ArtGenerator` - 75 edges
2. `ResourceGenerator` - 63 edges
3. `PackageManager` - 57 edges
4. `Main` - 42 edges
5. `CombatHUD` - 41 edges
6. `AchievementManager` - 39 edges
7. `TutorialManager` - 36 edges
8. `EnhancedMainMenu` - 32 edges
9. `GameManager` - 32 edges
10. `StsCombatEngine` - 31 edges

## Surprising Connections (you probably didn't know these)
- `CombatHUD` --inherits--> `IUIScreen`  [EXTRACTED]
  Client/GameModes/base_game/Code/UI/CombatHUD.cs →   _Bridges community 0 → community 12_
- `CharacterSelect` --inherits--> `Control`  [EXTRACTED]
  Client/GameModes/base_game/Code/UI/CharacterSelect.cs →   _Bridges community 0 → community 39_
- `AchievementPanel` --inherits--> `Control`  [EXTRACTED]
  Client/GameModes/base_game/Code/UI/Panels/AchievementPanel.cs →   _Bridges community 0 → community 97_
- `NetworkTest` --inherits--> `Control`  [EXTRACTED]
  Client/GameModes/base_game/Code/Tests/NetworkTest.cs →   _Bridges community 0 → community 82_
- `EnhancedMainMenu` --inherits--> `Control`  [EXTRACTED]
  Client/Scripts/UI/EnhancedMainMenu.cs →   _Bridges community 0 → community 14_

## Communities

### Community 0 - "Community 0"
Cohesion: 0.01
Nodes (44): AchievementPopup, RoguelikeGame.UI.Panels, BattleCharacterSprite, CardUI, CombatHUD, EnemyUnitUI, FloatingText, FloatingTextLabel (+36 more)

### Community 1 - "Community 1"
Cohesion: 0.03
Nodes (24): DamageInfo, DamageResult, DamageSystem, RoguelikeGame.Systems, DungeonData, DungeonGenerator, RoguelikeGame.Generation, EventBus (+16 more)

### Community 2 - "Community 2"
Cohesion: 0.03
Nodes (19): AcceptanceTests, RoguelikeGame.Tests, ConfigCompiler, RoguelikeGame.Editor, ConfigTest, RoguelikeGame.Tests, ConnectionManager, RoguelikeGame.Network.Core (+11 more)

### Community 3 - "Community 3"
Cohesion: 0.08
Nodes (2): ArtGenerator, RoguelikeGame.Core

### Community 4 - "Community 4"
Cohesion: 0.09
Nodes (2): ResourceGenerator, RoguelikeGame.Core

### Community 5 - "Community 5"
Cohesion: 0.07
Nodes (8): RoguelikeGame.Core, StsCardData, StsCombatEngine, StsEnemyData, StsEnemyIntent, StsPlayerState, StsPlayResult, StsStatusEffect

### Community 6 - "Community 6"
Cohesion: 0.08
Nodes (2): PackageManager, RoguelikeGame.Packages

### Community 7 - "Community 7"
Cohesion: 0.06
Nodes (15): Bullet, RoguelikeGame.Entities, Node2D, BlockSparkEffect, CardPlayEffect, DamageBurstEffect, DeathEffect, GoldPickupEffect (+7 more)

### Community 8 - "Community 8"
Cohesion: 0.1
Nodes (2): Main, RoguelikeGame

### Community 9 - "Community 9"
Cohesion: 0.09
Nodes (4): AchievementData, AchievementDefinition, AchievementManager, RoguelikeGame.Systems

### Community 10 - "Community 10"
Cohesion: 0.06
Nodes (4): BaseGameExtension, IPackageExtension, PackageExtensionBase, RoguelikeGame.Packages

### Community 11 - "Community 11"
Cohesion: 0.1
Nodes (3): RoguelikeGame.Systems, TutorialManager, TutorialStep

### Community 12 - "Community 12"
Cohesion: 0.09
Nodes (4): IUIScreen, MapNodeUI, MapView, RoguelikeGame.UI

### Community 13 - "Community 13"
Cohesion: 0.12
Nodes (3): GameManager, RoguelikeGame.Core, RunData

### Community 14 - "Community 14"
Cohesion: 0.1
Nodes (2): EnhancedMainMenu, RoguelikeGame.UI

### Community 15 - "Community 15"
Cohesion: 0.06
Nodes (13): AuthController, LoginRequest, RegisterRequest, RoguelikeGame.Server.Controllers, ControllerBase, LeaderboardController, RoguelikeGame.Server.Controllers, SubmitScoreRequest (+5 more)

### Community 16 - "Community 16"
Cohesion: 0.1
Nodes (6): RoguelikeGame.Core, StsBossData, StsCharacterData, StsEventData, StsEventOption, StsPotionData

### Community 17 - "Community 17"
Cohesion: 0.13
Nodes (4): BuildTool, main(), 构建包含 .NET 环境变量的环境 - 基于已注入的 os.environ, 线程安全的日志方法 - 使用 root.after() 在主线程更新GUI

### Community 18 - "Community 18"
Cohesion: 0.1
Nodes (3): RoguelikeGame.Core, StsRelicData, StsRelicManager

### Community 19 - "Community 19"
Cohesion: 0.16
Nodes (6): ArtAssetManager, AssetMapping, AssetSource, main(), 添加自定义美术资源（统一工作流入口）                  Args:             source_path: 源文件路径（可以是本地文件, 从目录批量导入资源                  Args:             source_dir: 源目录             target_

### Community 20 - "Community 20"
Cohesion: 0.12
Nodes (3): CombatManager, CombatState, RoguelikeGame.Core

### Community 21 - "Community 21"
Cohesion: 0.14
Nodes (2): NetworkSystemAcceptanceTests, RoguelikeGame.Tests

### Community 22 - "Community 22"
Cohesion: 0.14
Nodes (2): AudioManager, RoguelikeGame.Audio

### Community 23 - "Community 23"
Cohesion: 0.11
Nodes (5): RoguelikeGame.Systems, SkillData, SkillEffect, SkillInstance, SkillManager

### Community 24 - "Community 24"
Cohesion: 0.11
Nodes (5): CardAction, GameSessionManager, GameState, PlayerState, RoguelikeGame.Network.Session

### Community 25 - "Community 25"
Cohesion: 0.15
Nodes (24): call_doubao_api(), _draw_boss_enemy(), _draw_concert_bg(), _draw_effect(), _draw_forest_bg(), _draw_fragment(), _draw_game_icon(), _draw_heart() (+16 more)

### Community 26 - "Community 26"
Cohesion: 0.11
Nodes (4): FloorMap, MapGenerator, MapNode, RoguelikeGame.Generation

### Community 27 - "Community 27"
Cohesion: 0.14
Nodes (2): PackageStoreUI, RoguelikeGame.UI

### Community 28 - "Community 28"
Cohesion: 0.14
Nodes (3): RoguelikeGame.Systems, ShopItem, ShopManager

### Community 29 - "Community 29"
Cohesion: 0.14
Nodes (3): IUIScreen, RoguelikeGame.UI, UIManager

### Community 30 - "Community 30"
Cohesion: 0.09
Nodes (7): ENetConnectionAdapter, RoguelikeGame.Network.Core, IConnectionAdapter, RoguelikeGame.Network.Core, IDisposable, RoguelikeGame.Network.Core, WebRTCConnectionAdapter

### Community 31 - "Community 31"
Cohesion: 0.1
Nodes (3): IRoomService, RoguelikeGame.Server.Services, RoomService

### Community 32 - "Community 32"
Cohesion: 0.16
Nodes (5): AutoAssetDownloader, main(), 下载文件，按优先级尝试多种方法：         1. curl (最可靠)         2. wget         3. urllib (带SSL修复, 尝试使用 urllib 下载（带SSL证书问题修复）, ReliableDownloader

### Community 33 - "Community 33"
Cohesion: 0.13
Nodes (6): HealEffect, ItemData, ItemEffect, ItemManager, RoguelikeGame.Systems, StatModifierEffect

### Community 34 - "Community 34"
Cohesion: 0.13
Nodes (3): ObjectPool, ObjectPoolManager, RoguelikeGame.Systems

### Community 35 - "Community 35"
Cohesion: 0.16
Nodes (5): GameData, PlayerData, RoguelikeGame.Systems, SaveSlotInfo, SaveSystem

### Community 36 - "Community 36"
Cohesion: 0.14
Nodes (3): LANDiscoveryService, LANHostInfo, RoguelikeGame.Network.Discovery

### Community 37 - "Community 37"
Cohesion: 0.15
Nodes (5): PlayerInfo, RoguelikeGame.Network.Rooms, RoomInfo, RoomManager, RoomResult

### Community 38 - "Community 38"
Cohesion: 0.21
Nodes (2): KenneyStyleGenerator, main()

### Community 39 - "Community 39"
Cohesion: 0.14
Nodes (4): CharacterCardControl, CharacterSelect, RoguelikeGame.UI, PanelContainer

### Community 40 - "Community 40"
Cohesion: 0.1
Nodes (6): IAchievementUI, ICombatUI, IMapUI, IRestSiteUI, IShopUI, RoguelikeGame.UI

### Community 41 - "Community 41"
Cohesion: 0.15
Nodes (3): RoguelikeGame.Database, TimelineEntry, TimelineManager

### Community 42 - "Community 42"
Cohesion: 0.16
Nodes (3): RelicData, RelicDatabase, RoguelikeGame.Database

### Community 43 - "Community 43"
Cohesion: 0.1
Nodes (19): AudioConfigData, AudioSettingsConfig, CardConfig, CardConfigData, CharacterConfig, CharacterConfigData, EffectConfig, EffectConfigData (+11 more)

### Community 44 - "Community 44"
Cohesion: 0.16
Nodes (4): AudioConfig, AudioGenerator, RoguelikeGame.Audio, SFXConfig

### Community 45 - "Community 45"
Cohesion: 0.18
Nodes (2): EnemyAI, RoguelikeGame.Entities

### Community 46 - "Community 46"
Cohesion: 0.18
Nodes (2): LobbyPanel, RoguelikeGame.UI.Panels

### Community 47 - "Community 47"
Cohesion: 0.15
Nodes (2): MultiProtocolManager, RoguelikeGame.Network

### Community 48 - "Community 48"
Cohesion: 0.16
Nodes (4): AuthResult, AuthSystem, RoguelikeGame.Network.Auth, UserInfo

### Community 49 - "Community 49"
Cohesion: 0.12
Nodes (2): GameDatabaseManager, RoguelikeGame.Database

### Community 50 - "Community 50"
Cohesion: 0.19
Nodes (3): EnhancedSaveSystem, RoguelikeGame.Systems, SaveSlot

### Community 51 - "Community 51"
Cohesion: 0.14
Nodes (2): MainMenuNetworkIntegrator, RoguelikeGame.UI

### Community 52 - "Community 52"
Cohesion: 0.21
Nodes (2): GuaranteedAssetGenerator, main()

### Community 53 - "Community 53"
Cohesion: 0.15
Nodes (3): CombatNetworkSync, NetworkAction, RoguelikeGame.Core

### Community 54 - "Community 54"
Cohesion: 0.18
Nodes (2): NetworkManager, RoguelikeGame.Network

### Community 55 - "Community 55"
Cohesion: 0.18
Nodes (3): EnemyData, EnemyDatabase, RoguelikeGame.Database

### Community 56 - "Community 56"
Cohesion: 0.16
Nodes (3): CardData, CardDatabase, RoguelikeGame.Database

### Community 57 - "Community 57"
Cohesion: 0.12
Nodes (5): GameHub, RoguelikeGame.Server.Hubs, Hub, LobbyHub, RoguelikeGame.Server.Hubs

### Community 58 - "Community 58"
Cohesion: 0.23
Nodes (5): AssetFixer, main(), 将 Kenney 资源映射到配置文件需要的路径, 替换角色立绘为更好的版本或使用 Kenney 资源, 使用 Kenney 风格重新生成缺失的关键资源

### Community 59 - "Community 59"
Cohesion: 0.44
Nodes (8): generate_backgrounds(), generate_character_portraits(), generate_enemy_images(), generate_potion_images(), generate_relic_images(), generate_ui_icons(), ImageGenerator, main()

### Community 60 - "Community 60"
Cohesion: 0.27
Nodes (4): KenneyOfficialDownloader, main(), 从 Kenney.nl 页面提取真实的下载链接                  关键发现：Kenney 的下载链接格式为：         https://k, 处理单个资源包：提取URL → 下载 → 解压整合

### Community 61 - "Community 61"
Cohesion: 0.17
Nodes (4): AchievementData, AchievementSystem, RoguelikeGame.Database, RunStatistics

### Community 62 - "Community 62"
Cohesion: 0.19
Nodes (3): EncounterGenerator, EncounterResult, RoguelikeGame.Core

### Community 63 - "Community 63"
Cohesion: 0.17
Nodes (4): RoguelikeGame.Systems, WaveData, WaveEnemy, WaveManager

### Community 64 - "Community 64"
Cohesion: 0.17
Nodes (5): LootDrop, LootEntry, LootSystem, LootTable, RoguelikeGame.Systems

### Community 65 - "Community 65"
Cohesion: 0.29
Nodes (14): download_and_integrate_kenney_pack(), download_file(), extract_zip(), find_png_files(), generate_integration_report(), init_directories(), integrate_assets(), _is_placeholder() (+6 more)

### Community 66 - "Community 66"
Cohesion: 0.14
Nodes (5): IUIScreenController, IUIScreenController, RoguelikeGame.UI, MainMenuController, RoguelikeGame.UI

### Community 67 - "Community 67"
Cohesion: 0.24
Nodes (2): ResourceInitializer, RoguelikeGame.Core

### Community 68 - "Community 68"
Cohesion: 0.24
Nodes (3): NetworkPacket, PacketSerializer, RoguelikeGame.Network.Core

### Community 69 - "Community 69"
Cohesion: 0.15
Nodes (3): ILeaderboardService, LeaderboardService, RoguelikeGame.Server.Services

### Community 70 - "Community 70"
Cohesion: 0.27
Nodes (2): BuildTool, main()

### Community 71 - "Community 71"
Cohesion: 0.34
Nodes (3): main(), 使用多重 URL 故障转移的可靠下载器         按顺序尝试每个 URL，直到成功或全部失败, UltimateDownloader

### Community 72 - "Community 72"
Cohesion: 0.27
Nodes (2): main(), OneClickDownloader

### Community 73 - "Community 73"
Cohesion: 0.26
Nodes (3): AIAssetMatcher, main(), 清理 .import 缓存（不删除 .godot 目录，让 Godot 自行管理）

### Community 74 - "Community 74"
Cohesion: 0.2
Nodes (3): CharacterData, CharacterDatabase, RoguelikeGame.Database

### Community 75 - "Community 75"
Cohesion: 0.23
Nodes (2): RoguelikeGame.Core, StsCardDatabase

### Community 76 - "Community 76"
Cohesion: 0.15
Nodes (4): PackageExtensionBase, FrostExpansion, RoguelikeGame.Packages.Samples, ShadowRealmExtension

### Community 77 - "Community 77"
Cohesion: 0.19
Nodes (3): AuthService, IAuthService, RoguelikeGame.Server.Services

### Community 78 - "Community 78"
Cohesion: 0.27
Nodes (3): main(), 将 Godot Web 导出转换为微信小游戏          Args:             input_dir: Godot Web 导出的目录路径, WeChatConverter

### Community 79 - "Community 79"
Cohesion: 0.29
Nodes (12): batch_import(), categorize_by_filename(), categorize_file(), ensure_directories(), find_image_files(), generate_config_suggestions(), _get_category_usage(), _get_config_field_for_category() (+4 more)

### Community 80 - "Community 80"
Cohesion: 0.18
Nodes (3): CanvasLayer, HUD, RoguelikeGame.UI

### Community 81 - "Community 81"
Cohesion: 0.19
Nodes (2): ConfigLoader, RoguelikeGame.Core

### Community 82 - "Community 82"
Cohesion: 0.27
Nodes (1): NetworkTest

### Community 83 - "Community 83"
Cohesion: 0.24
Nodes (3): RoguelikeGame.Generation, Room, RoomGenerator

### Community 84 - "Community 84"
Cohesion: 0.23
Nodes (2): RandomGenerator, RoguelikeGame.Core

### Community 85 - "Community 85"
Cohesion: 0.2
Nodes (3): IMatchmakingService, MatchmakingService, RoguelikeGame.Server.Services

### Community 86 - "Community 86"
Cohesion: 0.32
Nodes (2): AudioGenerator, main()

### Community 87 - "Community 87"
Cohesion: 0.2
Nodes (3): CharacterBody2D, Player, RoguelikeGame.Entities

### Community 88 - "Community 88"
Cohesion: 0.27
Nodes (2): LoginPanel, RoguelikeGame.UI.Panels

### Community 89 - "Community 89"
Cohesion: 0.29
Nodes (2): LeaderboardPanel, RoguelikeGame.UI.Panels

### Community 90 - "Community 90"
Cohesion: 0.26
Nodes (2): ResourceManager, RoguelikeGame.Core

### Community 91 - "Community 91"
Cohesion: 0.27
Nodes (3): CDNHandler, create_test_directory_structure(), main()

### Community 92 - "Community 92"
Cohesion: 0.27
Nodes (2): MultiplayerPanel, RoguelikeGame.UI.Panels

### Community 93 - "Community 93"
Cohesion: 0.47
Nodes (8): apply_suggestions(), find_available_assets(), load_config(), main(), save_config(), suggest_card_updates(), suggest_character_updates(), suggest_enemy_updates()

### Community 94 - "Community 94"
Cohesion: 0.22
Nodes (8): ActConfig, AudioConfig, ContentConfig, DifficultyConfig, GameplayConfig, PackageConfig, RoguelikeGame.Packages, UIConfig

### Community 95 - "Community 95"
Cohesion: 0.29
Nodes (2): pollEvents(), updateStats()

### Community 96 - "Community 96"
Cohesion: 0.46
Nodes (7): clean_all_import_caches(), fix_filename_casing(), force_godot_reimport(), load_json(), main(), regenerate_all_assets(), verify_assets()

### Community 97 - "Community 97"
Cohesion: 0.39
Nodes (2): AchievementPanel, RoguelikeGame.UI.Panels

### Community 98 - "Community 98"
Cohesion: 0.39
Nodes (2): ConnectionStatusIndicator, RoguelikeGame.UI.Panels

### Community 99 - "Community 99"
Cohesion: 0.29
Nodes (0): 

### Community 100 - "Community 100"
Cohesion: 0.52
Nodes (6): create_directory_structure(), create_readme(), execute_migration(), generate_path_mapping(), main(), plan_migration()

### Community 101 - "Community 101"
Cohesion: 0.33
Nodes (5): CreateRoomRequest, JoinRoomRequest, RoguelikeGame.Shared.Protocol, RoomInfo, RoomPlayerInfo

### Community 102 - "Community 102"
Cohesion: 0.33
Nodes (5): PackageCategory, PackageData, PackageInstallState, PackageRegistry, RoguelikeGame.Packages

### Community 103 - "Community 103"
Cohesion: 0.4
Nodes (3): ApplicationDbContext, RoguelikeGame.Server.Data, DbContext

### Community 104 - "Community 104"
Cohesion: 0.4
Nodes (4): AuthRequest, AuthResponse, RoguelikeGame.Shared.Protocol, UserInfo

### Community 105 - "Community 105"
Cohesion: 0.4
Nodes (4): GameAction, GameResult, GameStateUpdate, RoguelikeGame.Shared.Protocol

### Community 106 - "Community 106"
Cohesion: 0.5
Nodes (4): build_monster(), copy_and_resize(), map_all_resources(), 用 Monster Builder Pack 部件组装怪物

### Community 107 - "Community 107"
Cohesion: 0.4
Nodes (2): CardStyleConfig, RoguelikeGame.Core

### Community 108 - "Community 108"
Cohesion: 0.5
Nodes (3): RoguelikeGame.Server.Models, Room, RoomPlayer

### Community 109 - "Community 109"
Cohesion: 0.83
Nodes (3): create_package(), generate_registry(), main()

### Community 110 - "Community 110"
Cohesion: 0.5
Nodes (2): analyze_kenney_page(), 分析 Kenney.nl 页面，提取下载信息

### Community 111 - "Community 111"
Cohesion: 0.67
Nodes (2): find_tiles(), make_background()

### Community 112 - "Community 112"
Cohesion: 0.67
Nodes (2): LeaderboardEntry, RoguelikeGame.Server.Models

### Community 113 - "Community 113"
Cohesion: 0.67
Nodes (2): RoguelikeGame.Server.Models, User

### Community 114 - "Community 114"
Cohesion: 1.0
Nodes (0): 

### Community 115 - "Community 115"
Cohesion: 1.0
Nodes (0): 

### Community 116 - "Community 116"
Cohesion: 1.0
Nodes (0): 

### Community 117 - "Community 117"
Cohesion: 1.0
Nodes (0): 

### Community 118 - "Community 118"
Cohesion: 1.0
Nodes (0): 

### Community 119 - "Community 119"
Cohesion: 1.0
Nodes (0): 

### Community 120 - "Community 120"
Cohesion: 1.0
Nodes (0): 

### Community 121 - "Community 121"
Cohesion: 1.0
Nodes (0): 

### Community 122 - "Community 122"
Cohesion: 1.0
Nodes (0): 

### Community 123 - "Community 123"
Cohesion: 1.0
Nodes (0): 

### Community 124 - "Community 124"
Cohesion: 1.0
Nodes (0): 

### Community 125 - "Community 125"
Cohesion: 1.0
Nodes (0): 

### Community 126 - "Community 126"
Cohesion: 1.0
Nodes (0): 

### Community 127 - "Community 127"
Cohesion: 1.0
Nodes (0): 

### Community 128 - "Community 128"
Cohesion: 1.0
Nodes (0): 

### Community 129 - "Community 129"
Cohesion: 1.0
Nodes (0): 

### Community 130 - "Community 130"
Cohesion: 1.0
Nodes (0): 

### Community 131 - "Community 131"
Cohesion: 1.0
Nodes (0): 

### Community 132 - "Community 132"
Cohesion: 1.0
Nodes (0): 

### Community 133 - "Community 133"
Cohesion: 1.0
Nodes (0): 

### Community 134 - "Community 134"
Cohesion: 1.0
Nodes (0): 

## Knowledge Gaps
- **244 isolated node(s):** `RoguelikeGame.Server.Models`, `LeaderboardEntry`, `RoguelikeGame.Server.Models`, `User`, `RoguelikeGame.Server.Models` (+239 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **Thin community `Community 114`** (2 nodes): `generate_icon.py`, `create_icon()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 115`** (1 nodes): `emit_test.py`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 116`** (1 nodes): `quick_test.py`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 117`** (1 nodes): `check_js2.py`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 118`** (1 nodes): `viz_standalone.js`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 119`** (1 nodes): `viz_check.js`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 120`** (1 nodes): `start_viz.py`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 121`** (1 nodes): `check_js3.py`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 122`** (1 nodes): `check_js.py`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 123`** (1 nodes): `Program.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 124`** (1 nodes): `RoguelikeGame.Server.MvcApplicationPartsAssemblyInfo.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 125`** (1 nodes): `RoguelikeGame.Server.AssemblyInfo.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 126`** (1 nodes): `RoguelikeGame.Server.GlobalUsings.g.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 127`** (1 nodes): `RoguelikeGame.Shared.GlobalUsings.g.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 128`** (1 nodes): `RoguelikeGame.Shared.AssemblyInfo.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 129`** (1 nodes): `map_ui_assets.py`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 130`** (1 nodes): `_diag2.py`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 131`** (1 nodes): `_diag.py`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 132`** (1 nodes): `download_more_packs.py`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 133`** (1 nodes): `generate_player_icon.py`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 134`** (1 nodes): `download_missing_packs.py`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `SingletonBase` connect `Community 2` to `Community 1`?**
  _High betweenness centrality (0.094) - this node is a cross-community bridge._
- **Why does `ArtGenerator` connect `Community 3` to `Community 1`?**
  _High betweenness centrality (0.030) - this node is a cross-community bridge._
- **Why does `ResourceGenerator` connect `Community 4` to `Community 1`?**
  _High betweenness centrality (0.025) - this node is a cross-community bridge._
- **What connects `RoguelikeGame.Server.Models`, `LeaderboardEntry`, `RoguelikeGame.Server.Models` to the rest of the system?**
  _244 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Community 0` be split into smaller, more focused modules?**
  _Cohesion score 0.01 - nodes in this community are weakly interconnected._
- **Should `Community 1` be split into smaller, more focused modules?**
  _Cohesion score 0.03 - nodes in this community are weakly interconnected._
- **Should `Community 2` be split into smaller, more focused modules?**
  _Cohesion score 0.03 - nodes in this community are weakly interconnected._