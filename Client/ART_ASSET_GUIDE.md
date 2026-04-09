# 🎨 Godot Roguelike 美术资源集成指南

## 📋 快速开始

### 1. 添加单个资源
```bash
python art_asset_manager.py add --source /path/to/image.png --category cards --name my_card.png
```

### 2. 批量导入目录
```bash
python art_asset_manager.py batch-import --source ./my_art/ --category skills --pattern "*.png"
```

### 3. 从 URL 下载并集成
```bash
python art_asset_manager.py add --source https://example.com/image.png --category relics --name cool_relic.png
```

### 4. 下载 Kenney 免费素材包
```bash
python art_asset_manager.py kenney-download
```

### 5. 查看资源状态
```bash
python art_asset_manager.py status
python art_asset_manager.py missing
```

## 📂 资源分类说明

| 分类代码 | 目录 | 用途 | 推荐尺寸 |
|---------|------|------|---------|
| `cards` | Icons/Cards/ | 卡牌图标 | 120×170px |
| `enemies_icon` | Icons/Enemies/ | 敌人头像 | 64×64px 或 128×128px |
| `relics` | Icons/Relics/ | 遗物图标 | 64×64px |
| `skills` | Icons/Skills/ | 技能图标 | 64×64px |
| `items` | Icons/Items/ | 物品图标 | 48×48px |
| `achievements` | Icons/Achievements/ | 成就图标 | 64×64px |
| `rest_sites` | Icons/Rest/ | 休息站图标 | 64×64px |
| `services` | Icons/Services/ | 服务图标 | 64×64px |
| `characters` | Images/Characters/ | 角色立绘 | 400×600px |
| `enemies_full` | Images/Enemies/ | 敌人战斗图 | 200×300px |
| `backgrounds` | Images/Backgrounds/ | 背景图 | 1920×1080px |
| `events` | Images/Events/ | 事件插图 | 400×300px |
| `potions` | Images/Potions/ | 药水图片 | 64×64px |

## 🔗 推荐免费资源网站

### Kenney.nl (CC0 免费商用)
- **网址**: https://kenney.nl/assets
- **推荐包**:
  - RPG Urban Pack (城市RPG素材)
  - Platformer Pack (平台跳跃素材)
  - UI Pack (界面元素)
  - Particle Effects (粒子特效)

### OpenGameArt (多种授权)
- **网址**: https://opengameart.org
- **推荐搜索关键词**: "rpg", "pixel art", "fantasy", "cards"

### 其他资源
- Itch.io 免费素材: https://itch.io/game-assets/free
- Game-Icons.net: https://game-icons.net (3000+ CC BY 3.0 图标)

## 🛠️ 工作流程

### 日常开发流程
1. ✅ 找到合适的免费素材
2. ✅ 使用 `add` 命令集成到项目
3. ✅ 更新 Config/Data/*.json 配置文件中的路径
4. ✅ 在 Godot 编辑器中验证显示效果
5. ✅ 提交版本控制

### 批量替换流程
1. ✅ 准备好新的素材集合
2. ✅ 使用 `batch-import` 导入
3. ✅ 运行 `missing` 检查遗漏
4. ✅ 更新 JSON 配置
5. ✅ 测试游戏功能

## 📝 配置文件格式

添加资源时可以附带元数据:

```json
{
  "author": "艺术家名称",
  "license": "CC0",
  "source_url": "原始下载链接",
  "notes": "使用说明"
}
```

## ⚠️ 注意事项

1. **授权合规**: 确保使用的素材符合项目授权要求
2. **尺寸规范**: 尽量遵循推荐尺寸以保证显示效果
3. **命名规范**: 使用英文小写+下划线命名
4. **备份机制**: 工具会自动备份被替换的原文件
5. **格式要求**: 优先使用 PNG 格式（支持透明通道）

## 📞 常见问题

**Q: 如何处理不同尺寸的资源?**
A: 工具会保持原始尺寸，建议在导入前使用图像编辑软件调整。

**Q: 支持哪些图片格式?**
A: PNG (推荐), JPG, WebP, BMP。

**Q: 如何撤销某个资源的替换?**
A: 查找 `.backup` 后缀的文件恢复即可。

---

最后更新: 2026-04-08 01:31:53
