# GDScript 4 编码规范与易错写法指南

> 本项目 GameModes 文件夹下**只允许使用 GDScript**，禁止 C# 代码。
> 所有变量和函数参数必须使用**显式类型声明**。

---

## 一、必须遵守的规则

### 1. 显式类型声明（强制）

所有变量声明必须显式标注类型，禁止省略类型推断：

```gdscript
# ✅ 正确 - 显式类型
var _player_sprite: Control = null
var hit_tween: Tween = enemy_ui.create_tween()
var config: Dictionary = ConfigLoader.load_config("cards")
var x: float = float(node_data.position.x)

# ❌ 错误 - 类型推断（GDScript静态检查可能无法推断）
var _player_sprite: Node2D = null        # 赋值Control类型会报错
var hit_tween := enemy_ui.create_tween()   # 无法推断Tween类型
```

### 2. 内部类（嵌套类）语法

GDScript 4 中，嵌套在另一个类内部的子类，**必须使用两行格式 + 成员缩进**：

```gdscript
# ✅ 正确写法
class MapNodeUI:
	extends Control

	var data: int = 0                    # 类体成员必须额外缩进一层！

	func _ready() -> void:               # 函数也必须额外缩进！
		pass

# ❌ 错误写法 - 单行extends（会导致 Parse Error）
class MapNodeUI extends Control:
	var data: int = 0                   # 这些被解析为外部类成员！
```

**注意**: IDE linter 可能会将两行格式自动回退为单行格式。如果遇到此问题，需要手动修正。

### 3. 禁止引用 C# 单例/类

GameModes 中的 GDScript **不能直接引用 C# 类**（如 `Main`、`AudioManager` 等），除非它们已注册为 autoload。

```gdscript
# ❌ 错误 - Main是C#类，GDScript无法直接引用
if Main.instance != null:
	Main.instance.go_to_main_menu()

# ✅ 正确 - 通过场景树查找或信号通信
var main_node := get_tree().root.get_node_or_null("/root/Main") as Node
if main_node != null and main_node.has_method("go_to_main_menu"):
	main_node.call("go_to_main_menu")
elif get_tree().current_scene != null:
	get_tree().change_scene_to_file("res://Scenes/Main.tscn")
```

### 4. Autoload 引用

只有注册在 `project.godot` → `[autoload]` 中的脚本才能全局访问：

```
[autoload]
ConfigLoader="*res://GameModes/base_game/Scripts/config_loader.gd"
CardDatabase="*res://GameModes/base_game/Scripts/cards/card_database.gd"
```

未注册的类引用会报 `Identifier "XXX" not declared in the current scope`。

---

## 二、常见易错写法汇总

| 错误类型 | 错误示例 | 正确写法 |
|---------|---------|---------|
| **内部类无缩进** | `class A:\n\texttends B\nvar x = 1` | `class A:\n\texttends B\n\tvar x = 1` |
| **类型不匹配** | `var s: Node2D = Control.new()` | `var s: Control = Control.new()` |
| **缺少显式类型** | `var t := create_tween()` | `var t: Tween = create_tween()` |
| **C#类引用** | `Main.instance.xxx()` | `get_tree().root.get_node_or_null(...)` |
| **Node访问CanvasItem属性** | `var n: Node; n.visible = true` | `var ci: CanvasItem = n as CanvasItem; if ci: ci.visible = true` |
| **Dictionary返回null** | `func f() -> Dictionary:\n\treturn null` | `return {}` |
| **方法名错误** | `CardDatabase.get_total_cards()` | `CardDatabase.total_cards()` |
| **slice()参数错误** | `arr.slice(start, count)` | `arr.slice(start)` 或 `arr.slice(start, start+count)` |
| **C# partial class与GDScript class_name同名** | C#端声明`partial class GameOverScreen : Control`，GDScript端声明`class_name GameOverScreen` | GameModes下不允许C#代码，C#端不应声明与GDScript class_name同名的partial class |
| **VBoxContainer不支持panel样式** | `vbox.add_theme_stylebox_override("panel", style)` | 使用`PanelContainer`包裹VBoxContainer，在PanelContainer上设置样式 |
| **Tween类型需显式声明** | `var tween := create_tween()` | `var tween: Tween = create_tween()` |
| **Texture2D类型需显式声明** | `var tex := load(path) as Texture2D` | `var tex: Texture2D = load(path) as Texture2D` |
| **GDScript autoload无instance属性** | `AudioManager.instance.play_button_click()` | GDScript autoload直接用全局名调用: `AudioManager.play_button_click()` |
| **Dictionary与自定义对象混用** | `character.character` (Dictionary无此key) | 先判断类型: `if character is Dictionary: character.get("class", 0) else: character.character` |
| **未初始化变量引用** | `func update_player_info(): _player_info_bar.get_children()` | 确保变量在_ready或_build_ui中初始化后再引用 |
| **硬编码字符串** | `_title.text = "杀戮尖塔 2"` | 使用常量: `const GAME_TITLE: String = "杀戮尖塔 2"; _title.text = GAME_TITLE` |
| **信号未声明** | `emit_signal("form_changed", new_form)` | 必须先声明: `signal form_changed(new_form: String)` |
| **枚举值硬编码** | `match card.type: 0: _execute_attack()` | 使用枚举名: `match card.type: CardType.ATTACK: _execute_attack()` |
| **Dictionary访问未用get** | `card.type` (Dictionary无type属性) | `card.get("type", 0)` 安全访问并设默认值 |
| **逻辑条件反转** | `if _is_reviving and _partner_alive:` (应为not) | 仔细检查布尔条件语义: `if _is_reviving and not _partner_alive:` |
| **C#事件未取消订阅** | C#面板订阅RoomManager事件后QueueFree未取消 | 必须在_ExitTree中-=取消订阅，否则已释放对象被回调崩溃 |
| **HotPatchService引用方式** | `HotPatchService.instance.check_for_updates()` | GDScript autoload通过场景树: `get_node_or_null("/root/HotPatchService")` |
| **热更新路径混用** | 直接写入`res://GameModes/xxx` | 热更新文件写入`user://hotfix/{pkg_id}/`，读取时先查hotfix再查res:// |
| **HTTPRequest回调未释放** | `add_child(http)` 但不`queue_free()` | 必须在`request_completed`回调中调用`http.queue_free()` |
| **ZIPReader未关闭** | `var r := ZIPReader.new(); r.open(p)` | 使用后必须`r.close()`，否则文件锁不释放 |
| **FileAccess写入res://** | `FileAccess.open("res://xxx", WRITE)` | `res://`在发布后只读，热更新只能写入`user://` |
| **多人模式Seed未传递** | `dungeon_generator.generate_dungeon(floor, 0)` | 通过`MultiplayerSeedBridge.get_effective_seed()`获取多人种子 |
| **存档路径不统一** | `user://saves/slot_1.json` vs `user://saves/base_game_slot_1.json` | 统一使用`{package_id}_slot_{slot}.json`格式 |
| **服务器地址硬编码** | `"http://127.0.0.1:5002"` | 使用`ServerConfig.get_server_url()`从配置读取 |
| **PackedByteArray无sha256方法** | `data.sha256_text()` 或 `data.sha256_buffer().hex_encode()` | 使用`FileAccess.get_sha256(file_path)`直接计算文件哈希 |
| **_print不存在** | `_print("msg")` | GDScript内置是`print("msg")`，无下划线前缀 |
| **私有方法调用名不匹配** | `_rollback_update()` 但函数定义是`rollback_update()` | 内部回滚应提取为`_perform_rollback()`私有方法 |
| **C#信号GDScript未监听** | C# `EmitSignal("MultiplayerSeedReceived", seed)` | GDScript需`session_manager.MultiplayerSeedReceived.connect(_on_seed)` |

---

## 三、类型安全转换模式

当不确定对象具体类型时，使用安全转换：

```gdscript
# Node -> CanvasItem (访问visible等属性)
var obj_ci: CanvasItem = some_node as CanvasItem
if obj_ci != null:
	obj_ci.visible = true

# Node -> Node2D (访问position等属性)
var obj_2d: Node2D = some_node as Node2D
if obj_2d != null:
	var pos: Vector2 = obj_2d.global_position

# 通用节点调用方法
if some_node.has_method("some_method"):
	some_node.call("some_method")
```

---

## 四、Autoload 注册清单

当前已注册的全局单例（可在任意GDScript中直接用名称访问）：

| 名称 | 路径 | 用途 |
|------|------|------|
| `ConfigLoader` | `config_loader.gd` | JSON/Bytes配置加载 |
| `PackageUIRegistry` | `package_ui_provider.gd` | UI包注册 |
| `PackageService` | `Scripts/Services/package_service.gd` | 包管理服务 |
| `HotPatchService` | `Scripts/Services/hot_patch_service.gd` | 热更新服务 |
| `EventBus` | `Core/EventBus.cs` | 全局事件总线(C#) |
| `MultiplayerSeedBridge` | `Scripts/Network/multiplayer_seed_bridge.gd` | 多人游戏种子桥接 |
| `ServerConfig` | `Scripts/Network/server_config.gd` | 服务器地址配置 |

如需新增 autoload，在 `project.godot` 的 `[autoload]` 段添加。

---

## 五、调试检查清单

每次修改 GDScript 后执行：

1. **dotnet build** - 检查C#编译（0错误）
2. **VSCode诊断** - 检查GDScript文件 diagnostics（0错误）
3. **Godot编辑器** - 运行游戏确认无运行时报错
