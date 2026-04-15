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
| `EventBus` | `Core/EventBus.cs` | 全局事件总线(C#) |

如需新增 autoload，在 `project.godot` 的 `[autoload]` 段添加。

---

## 五、调试检查清单

每次修改 GDScript 后执行：

1. **dotnet build** - 检查C#编译（0错误）
2. **VSCode诊断** - 检查GDScript文件 diagnostics（0错误）
3. **Godot编辑器** - 运行游戏确认无运行时报错
