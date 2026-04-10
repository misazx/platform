#!/usr/bin/env python3
"""
🔧 彻底资源修复器 v9.0 - 解决资源未生效问题
==============================================

根本原因：
1. .import 缓存被 Godot 运行时重新生成
2. 文件名大小写与配置不匹配
3. 存在重复/旧文件干扰

解决方案：
1. 删除所有 .import 缓存（递归）
2. 统一文件名为配置引用的格式
3. 删除重复/旧文件
4. 强制 Godot 重新导入
"""

import os
import sys
import json
import shutil
from pathlib import Path
from datetime import datetime

try:
    from PIL import Image, ImageDraw, ImageFont
    HAS_PIL = True
except ImportError:
    os.system("pip3 install Pillow -q")
    from PIL import Image, ImageDraw, ImageFont
    HAS_PIL = True


PROJECT_ROOT = Path(__file__).parent.parent / "GameModes" / "base_game" / "Resources"

def clean_all_import_caches():
    """递归删除所有 .import 缓存文件"""
    print("\n🧹 第1步：彻底清理 .import 缓存...")
    
    count = 0
    for pattern in ["**/*.import", "**/*.stex", "**/*.tex"]:
        for f in PROJECT_ROOT.glob(pattern):
            if f.is_file():
                f.unlink()
                count += 1
                print(f"   🗑️  删除: {f.relative_to(PROJECT_ROOT)}")
    
    # 也清理 .godot/imported 目录
    godot_imported = PROJECT_ROOT / ".godot" / "imported"
    if godot_imported.exists():
        shutil.rmtree(godot_imported)
        count += 1
        print(f"   🗑️  删除整个目录: .godot/imported/")
    
    print(f"   ✅ 共清理 {count} 个缓存文件/目录")
    return count

def fix_filename_casing():
    """修正文件名大小写以匹配配置文件"""
    print("\n🔤 第2步：修正文件名大小写...")
    
    fixes = []
    
    # 敌人图标 - 配置使用小写
    enemy_icon_mapping = {
        "Icons/Enemies/Cultist.png": "Icons/Enemies/cultist.png",
        "Icons/Enemies/JawWorm.png": "Icons/Enemies/jaw_worm.png",
        "Icons/Enemies/Lagavulin.png": "Icons/Enemies/lagavulin.png",
        "Icons/Enemies/TheGuardian.png": "Icons/Enemies/the_guardian.png",
    }
    
    # 敌人全身图 - 配置使用混合大小写
    enemy_full_mapping = {
        "Images/Enemies/cultist.png": "Images/Enemies/Cultist.png",
        "Images/Enemies/jaw_worm.png": "Images/Enemies/JawWorm.png",
        "Images/Enemies/lagavulin.png": "Images/Enemies/Lagavulin.png",
        "Images/Enemies/the_guardian.png": "Images/Enemies/TheGuardian.png",
    }
    
    # 应用映射
    for wrong, correct in {**enemy_icon_mapping, **enemy_full_mapping}.items():
        wrong_path = PROJECT_ROOT / wrong
        correct_path = PROJECT_ROOT / correct
        
        if wrong_path.exists() and not correct_path.exists():
            # macOS 需要特殊处理来重命名（改变大小写）
            temp_path = wrong_path.with_suffix('.temp_rename')
            wrong_path.rename(temp_path)
            temp_path.rename(correct_path)
            fixes.append(f"{wrong} → {correct}")
            print(f"   ✏️  重命名: {wrong} → {correct}")
        elif correct_path.exists():
            # 正确名称已存在，删除错误的
            if wrong_path.exists() and wrong_path != correct_path:
                wrong_path.unlink()
                fixes.append(f"删除重复: {wrong}")
                print(f"   🗑️  删除重复: {wrong}")
    
    # 删除其他可能的重复文件
    duplicates = [
        "Icons/Enemies/theguardian.png",
        "Icons/Enemies/jawworm.png",
        "Images/Enemies/theguardian.png",
        "Images/Enemies/jawworm.png",
    ]
    
    for dup in duplicates:
        dup_path = PROJECT_ROOT / dup
        if dup_path.exists():
            dup_path.unlink()
            fixes.append(f"删除重复: {dup}")
            print(f"   🗑️  删除重复: {dup}")
    
    print(f"   ✅ 修正 {len(fixs)} 个文件名问题")
    return fixes

def regenerate_all_assets():
    """重新生成所有资源（确保覆盖）"""
    print("\n🎨 第3步：重新生成 Kenney 风格资源...")
    
    # 导入生成器
    sys.path.insert(0, str(PROJECT_ROOT))
    from kenney_style_replace import KenneyStyleGenerator
    
    generator = KenneyStyleGenerator()
    generator.process_all_configs()

def force_godot_reimport():
    """强制 Godot 重新导入所有资源"""
    print("\n⚡ 第4步：创建强制重导入标记...")
    
    # 创建 .godot/imported 目录的占位符（如果不存在）
    godot_dir = PROJECT_ROOT / ".godot"
    godot_dir.mkdir(exist_ok=True)
    
    # 创建 import 标记文件
    marker = godot_dir / "force_reimport.marker"
    marker.write_text(f"Force reimport at {datetime.now()}\n")
    
    print(f"   ✅ 已创建重导入标记: {marker}")

def verify_assets():
    """验证所有资源文件存在且正确"""
    print("\n✅ 第5步：验证资源完整性...")
    
    issues = []
    
    # 检查卡牌
    cards_config = load_json("cards.json")
    if cards_config:
        for card in cards_config.get("cards", []):
            icon_path = card.get("iconPath", "").replace("res://", "")
            full_path = PROJECT_ROOT / icon_path
            if not full_path.exists():
                issues.append(f"❌ 缺少卡牌图标: {icon_path}")
            else:
                print(f"   ✅ {icon_path} ({full_path.stat().st_size} bytes)")
    
    # 检查角色
    chars_config = load_json("characters.json")
    if chars_config:
        for char in chars_config.get("characters", []):
            portrait_path = char.get("portraitPath", "").replace("res://", "")
            full_path = PROJECT_ROOT / portrait_path
            if not full_path.exists():
                issues.append(f"❌ 缺少角色立绘: {portrait_path}")
            else:
                print(f"   ✅ {portrait_path} ({full_path.stat().st_size} bytes)")
    
    # 检查敌人
    enemies_config = load_json("enemies.json")
    if enemies_config:
        for enemy in enemies_config.get("enemies", []):
            for path_key in ["iconPath", "portraitPath"]:
                path = enemy.get(path_key, "").replace("res://", "")
                full_path = PROJECT_ROOT / path
                if not full_path.exists():
                    issues.append(f"❌ 缺少敌人{path_key}: {path}")
                else:
                    print(f"   ✅ {path} ({full_path.stat().st_size} bytes)")
    
    if issues:
        print(f"\n⚠️  发现 {len(issues)} 个问题:")
        for issue in issues:
            print(f"   {issue}")
        return False
    else:
        print(f"\n✅ 所有资源文件验证通过！")
        return True

def load_json(name: str):
    """加载 JSON 配置文件"""
    path = PROJECT_ROOT / "Config" / "Data" / name
    if path.exists():
        with open(path, 'r', encoding='utf-8') as f:
            return json.load(f)
    return None

def main():
    print("""
╔═══════════════════════════════════════════════════════════╗
║                                                           ║
║   🔧 彻底资源修复器 v9.0                                  ║
║                                                           ║
║   解决资源未生效问题 - 彻底清理并重建                      ║
║                                                           ║
╚═══════════════════════════════════════════════════════════╝
""")
    
    # Step 1: 清理缓存
    cache_count = clean_all_import_caches()
    
    # Step 2: 修正文件名
    fixes = fix_filename_casing()
    
    # Step 3: 重新生成资源
    regenerate_all_assets()
    
    # Step 4: 强制重导入标记
    force_godot_reimport()
    
    # Step 5: 验证
    success = verify_assets()
    
    # 总结
    print(f"\n{'='*70}")
    print(f"🎉 彻底修复完成!")
    print(f"{'='*70}\n")
    
    print(f"📊 修复统计:")
    print(f"   • 清理缓存: {cache_count} 个文件")
    print(f"   • 修正文件名: {len(fixes)} 个")
    print(f"   • 资源验证: {'通过 ✅' if success else '失败 ❌'}")
    
    print(f"\n🚀 下一步操作（必须执行）:")
    print(f"   1. 完全关闭 Godot 编辑器")
    print(f"   2. 删除 .godot 目录（可选但推荐）:")
    print(f"      rm -rf .godot")
    print(f"   3. 重新打开项目")
    print(f"   4. 等待 Godot 完成资源导入（左下角进度条）")
    print(f"   5. 按 F5 运行游戏查看效果")
    
    print(f"\n💡 如果仍然显示旧资源:")
    print(f"   • 菜单栏 → Project → ReImport All")
    print(f"   • 或重启电脑后再打开项目")

if __name__ == "__main__":
    main()
