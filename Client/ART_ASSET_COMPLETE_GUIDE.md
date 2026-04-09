# 🎨 Godot Roguelike 项目 - 美术资源替换完整指南

## ✅ 已完成的自动化工具

你的项目现在拥有以下**完整的美术资源管理系统**：

| 工具文件 | 功能 | 使用场景 |
|---------|------|---------|
| `art_asset_manager.py` | 完整的资源管理器（支持下载、映射、统计） | 日常资源管理 |
| `quick_start_art.py` | 一键快速启动脚本 | 首次设置 |
| `batch_import.py` | **批量导入已下载的素材**（推荐⭐） | **日常使用** |

---

## 🚀 快速开始（3步完成）

### 第1步：下载免费素材包

#### 方法A：Kenney.nl (CC0 免费商用 - 推荐)

访问 https://kenney.nl/assets ，推荐下载以下素材包：

1. **RPG Urban Pack** 
   - 包含：角色精灵图、UI元素、道具、武器、特效
   - 适用：卡牌图标、角色立绘、物品图标
   - 大小：~15MB

2. **UI Pack**
   - 包含：按钮、面板、框架、进度条
   - 适用：界面UI、遗物边框
   - 大小：~8MB

3. **Platformer Characters**
   - 包含：多个角色/敌人的精灵图集
   - 适用：敌人形象、NPC
   - 大小：~5MB

4. **Particle Effects**
   - 包含：攻击、魔法、爆炸等粒子效果
   - 适用：技能特效
   - 大小：~2MB

**下载方法：**
- 进入页面后点击 "Download Asset Pack" 按钮
- 选择 ZIP 格式下载
- 保存到任意位置（如 `~/Downloads/`）

---

#### 方法B：OpenGameArt (多种开源授权)

访问 https://opengameart.org ，搜索以下关键词：

**推荐搜索词：**
- `rpg icons` - RPG 图标集
- `pixel art characters` - 像素艺术角色
- `fantasy ui` - 奇幻风格UI
- `card game assets` - 卡牌游戏素材
- `spell effects` - 法术特效

**热门推荐资源：**

1. **LPC (Liberated Pixel Cup) 角色**
   - 搜索: "lpc characters"
   - 授权: CC-BY-SA 3.0 / GPL 3.0
   - 用途：高质量像素角色

2. **Fantasy UI Icon Set**
   - 搜索: "fantasy ui icons"
   - 授权: 多种选择
   - 用途：遗物、技能图标

3. **RPG Item Sprites**
   - 搜索: "rpg items"
   - 授权: CC0 / CC-BY
   - 用途：物品、药水图标

**下载方法：**
- 找到喜欢的资源后，点击下载按钮
- 通常提供单个文件或ZIP打包下载
- 注意查看授权要求（大部分可免费商用）

---

### 第2步：使用批量导入工具整理素材

假设你已经将素材下载到 `~/Downloads/kenney-rpg-pack/`

```bash
# 基础用法 - 智能分类导入
python3 batch_import.py ~/Downloads/kenney-rpg-pack

# 导入到指定分类
python3 batch_import.py ~/Downloads/card-icons cards

# 同时导入多个目录
python3 batch_import.py ~/Downloads/pack1 ~/Downloads/pack2 ~/Downloads/pack3

# 查看所有可用分类
python3 batch_import.py --list-categories
```

**智能分类说明：**
工具会根据文件名自动识别资源类型：
- 包含 `card/attack/sword/shield` → 卡牌图标
- 包含 `player/character/warrior` → 角色立绘
- 包含 `enemy/monster/goblin` → 敌人形象
- 包含 `relic/artifact/ui/icon` → 遗物/UI图标
- 包含 `effect/spark/particle` → 技能特效
- 包含 `background/bg/tileset` → 背景图

---

### 第3步：更新游戏配置文件

导入完成后，编辑 `Config/Data/` 下的 JSON 文件：

#### 更新卡牌配置 (`cards.json`)
```json
{
  "id": "strike_ironclad",
  "iconPath": "res://Icons/Cards/新打击图标.png",  // ← 改为新路径
  ...
}
```

#### 更新角色配置 (`characters.json`)
```json
{
  "id": "ironclad",
  "portraitPath": "res://Images/Characters/新铁甲立绘.png",  // ← 改为新路径
  ...
}
```

#### 更新敌人配置 (`enemies.json`)
```json
{
  "id": "cultist",
  "portraitPath": "res://Images/Enemies/新邪教徒.png",  // ← 改为新路径
  "iconPath": "res://Icons/Enemies/新邪教徒图标.png",    // ← 改为新路径
  ...
}
```

**快速查找可用资源：**
```bash
# 查看刚导入的所有资源
cat Assets_Library/import_report.json

# 查看按分类整理的建议
cat Assets_Library/config_suggestions.json
```

---

## 📋 资源需求清单（当前项目）

根据配置文件分析，你需要准备以下资源：

### 必需资源（高优先级）

| 类型 | 数量 | 当前状态 | 推荐尺寸 | 来源建议 |
|-----|------|---------|---------|---------|
| **卡牌图标** | ~35张 | ⚠️ 占位符 | 120×170px | Kenney RPG + Game-Icons.net |
| **角色立绘** | 5个 | ⚠️ 火柴人 | 400×600px | LPC Characters / AI生成 |
| **敌人头像+全身** | 4组(8张) | ⚠️ 简单图形 | 头像64×64, 全身200×300 | Kenney Platformer |
| **遗物图标** | ~24张 | ⚠️ 基础形状 | 64×64px | Kenney UI / Fantasy Icons |
| **技能图标** | 4张+ | ⚠️ 纯色圆点 | 64×64px | Kenney Effects / 手绘 |

### 补充资源（中优先级）

| 类型 | 数量 | 说明 | 推荐来源 |
|-----|------|------|---------|
| **背景图** | 4-6张 | 各场景背景 | Kenney Tilesets / AI生成 |
| **事件插图** | ~12张 | 随机事件配图 | OpenGameArt 场景 |
| **药水/物品** | ~8张 | 消耗品图标 | Kenney Items |
| **成就图标** | 4张+ | 成就解锁图标 | 自定义设计 |

### 可选增强（低优先级）

| 类型 | 建议 |
|-----|------|
| **卡牌边框** | 区分稀有度（普通/稀有/史诗）|
| **动画帧** | 角色攻击/受伤动画（如需要）|
| **UI主题** | 统一的界面视觉风格 |
| **字体** | 游戏专用艺术字体 |

---

## 🔧 高级用法

### 批量重命名规则

如果下载的素材命名不规范，可以先重命名再导入：

```bash
# 示例：在源目录中统一命名规范
cd ~/Downloads/my-assets
rename 's/ /_/g' *          # 空格转下划线
rename 'y/A-Z/a-z/' *      # 转小写
```

### 使用 art_asset_manager.py 的其他功能

```bash
# 初始化系统
python3 art_asset_manager.py init

# 查看资源统计
python3 art_asset_manager.py status

# 检查缺失资源（对比配置文件）
python3 art_asset_manager.py missing

# 添加单个自定义资源
python3 art_asset_manager.py add \
  --source my_custom_art.png \
  --category relics \
  --name cool_relic.png

# 从URL直接添加
python3 art_asset_manager.py add \
  --source https://example.com/image.png \
  --category cards \
  --name new_card.png
```

### 后续添加新资源的标准流程

每次获得新的美术素材时：

```bash
# 1. 将素材放入临时目录
mkdir -p ~/temp_new_assets
cp 新素材.png ~/temp_new_assets/

# 2. 运行批量导入
python3 batch_import.py ~/temp_new_assets

# 3. 检查结果
cat Assets_Library/import_report.json

# 4. 更新对应JSON配置
# （手动编辑或写脚本批量更新）

# 5. Godot 中验证
# 打开项目 → 运行 → 检查显示
```

---

## 💡 实用技巧

### 技巧1：AI 辅助生成缺失资源

对于特殊需求（如特定风格的立绘），可以使用 AI 工具：

**Midjourney 提示词模板：**
```
Pixel art character portrait, fantasy warrior, [角色描述],
RPG game asset, clean lines, transparent background, 
high quality, game sprite --ar 9:16 --niji 5
```

**Stable Diffusion 提示词模板：**
```
pixelart, (game icon:1.2), [物品描述], 
rpg item, simple design, white background,
flat color, 64x64 pixel perfect
```

### 技巧2：保持视觉一致性

- **统一色调**：所有图标使用相似的配色方案
- **统一尺寸**：同类型资源保持相同分辨率
- **统一风格**：不要混合像素风和矢量风
- **统一光源**：所有图标的光照方向一致

### 技巧3：版本控制最佳实践

```bash
# .gitignore 中添加（如果需要）
Assets_Library/_downloads/
*.backup

# 但要提交实际的美术资源
Icons/
Images/
```

---

## ❓ 常见问题

**Q: 下载的素材有版权问题吗？**
A: Kenney.nl 的素材是 CC0（完全免费商用）。OpenGameArt 的素材授权各不相同，请查看每个资源的具体授权说明。

**Q: 如何处理不同尺寸的图片？**
A: 工具会保持原始尺寸。建议在导入前用图像编辑软件调整。Godot 会自动缩放显示。

**Q: 可以混合使用不同来源的素材吗？**
A: 可以，但要注意视觉风格统一。建议优先选择同一套素材包。

**Q: 替换后游戏报错怎么办？**
A: 检查 JSON 配置中的路径是否正确，确保文件存在且格式为 PNG/JPG。

**Q: 如何撤销某个资源的替换？**
A: 工具会自动备份原文件为 `.backup` 后缀，恢复即可。

---

## 📞 下一步

1. ✅ 从 Kenney.nl 下载至少 2 个素材包
2. ✅ 运行 `python3 batch_import.py <下载目录>`
3. ✅ 查看 `Assets_Library/config_suggestions.json`
4. ✅ 编辑 Config/Data/*.json 更新路径
5. ✅ 在 Godot 中验证效果
6. ✅ 根据需要补充更多资源

---

**最后更新**: 2026-04-08  
**适用项目**: Godot Roguelike Card Game  
**工具版本**: v1.0.0
