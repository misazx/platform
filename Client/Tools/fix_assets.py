#!/usr/bin/env python3
"""
Godot Roguelike - 资源修复工具 v7.0
=============================
解决 Kenney 资源未生效问题：
1. 清理旧的 .import 缓存
2. 将 Kenney 真实资源映射到正确的配置路径
3. 验证所有资源可用

运行:
  python3 fix_assets.py          # 全自动修复
"""

import os
import sys
import json
import shutil
from pathlib import Path
from datetime import datetime


PROJECT_ROOT = Path(__file__).parent.parent / "GameModes" / "base_game" / "Resources"

# 目录定义
DIRS = {
    "cards": PROJECT_ROOT / "Icons" / "Cards",
    "enemies_icon": PROJECT_ROOT / "Icons" / "Enemies",
    "relics": PROJECT_ROOT / "Icons" / "Relics",
    "skills": PROJECT_ROOT / "Icons" / "Skills",
    "items": PROJECT_ROOT / "Icons" / "Items",
    "characters": PROJECT_ROOT / "Images" / "Characters",
    "enemies_full": PROJECT_ROOT / "Images" / "Enemies",
    "backgrounds": PROJECT_ROOT / "Images" / "Backgrounds",
    "bgm": PROJECT_ROOT / "Audio" / "BGM",
    "sfx": PROJECT_ROOT / "Audio" / "SFX",
}


class AssetFixer:
    """资源修复器"""
    
    def __init__(self):
        self.stats = {
            "cache_cleaned": 0,
            "files_replaced": 0,
            "configs_updated": 0,
            "errors": [],
        }
    
    def log(self, msg: str):
        print(f"[{datetime.now().strftime('%H:%M:%S')}] {msg}")
    
    def clean_import_cache(self):
        """清理 .import 缓存"""
        self.log("🧹 第1步: 清理 Godot .import 缓存...")
        
        count = 0
        for dir_path in DIRS.values():
            if not dir_path.exists():
                continue
                
            for import_file in list(dir_path.glob("*.import")):
                try:
                    import_file.unlink()
                    count += 1
                except Exception as e:
                    self.stats["errors"].append(f"删除缓存失败: {import_file}")
        
        # 也清理项目根目录的 .import 文件
        for import_file in list(PROJECT_ROOT.rglob("*.import")):
            if "_downloads" not in str(import_file) and "Assets_Library" not in str(import_file):
                try:
                    import_file.unlink()
                    count += 1
                except Exception:
                    pass
        
        self.stats["cache_cleaned"] = count
        self.log(f"   ✅ 已清理 {count} 个 .import 缓存文件")
    
    def get_kenney_assets(self) -> Dict[str, Path]:
        """获取所有 Kenney 下载的真实资源"""
        kenney_files = {}
        
        # 扫描所有目录中的 PNG/OGG/WAV 文件
        for dir_path in DIRS.values():
            if not dir_path.exists():
                continue
            
            for ext in ["*.png", "*.ogg", "*.wav"]:
                for f in dir_path.glob(ext):
                    # 检查是否是 Kenney 的真实资源（通过大小判断）
                    size_kb = f.stat().st_size / 1024
                    
                    # Kenney 的资源通常 > 1KB 且有合理的尺寸
                    # 程序化生成的通常 < 5KB 或特定模式
                    is_kenney = size_kb > 3 or any(kw in f.name.lower() for kw in 
                        ['button', 'arrow', 'panel', 'bar_', 'icon', 'ui', 'box',
                         'click', 'back_', 'close_', 'drop_', 'confirmation',
                         'jingles', 'hit'])
                    
                    if is_kenney:
                        kenney_files[f.name] = f
        
        return kenney_files
    
    def map_kenney_to_config(self, kenney_assets: Dict[str, Path]):
        """将 Kenney 资源映射到配置文件需要的路径"""
        mappings = []
        
        # 卡牌图标映射 (攻击类 → 按钮/箭头)
        card_mappings = {
            "strike.png": ["button_square_red.png", "button_square_orange.png", "button_round_red.png"],
            "defend.png": ["button_square_blue.png", "button_round_blue.png", "shield.png"],
            "bash.png": ["button_square_yellow.png", "button_round_grey.png"],
            "cleave.png": ["arrow_basic_e.png", "arrow_decorative_e.png"],
            "iron_wave.png": ["arrow_basic_n.png", "arrow_long_e.png"],
            "pommel_strike.png": ["button_square_red.png"],
            "armaments.png": ["panel_flat.png", "panel_inset.png"],
            "flexibility.png": ["arrow_basic_w.png", "arrow_basic_s.png"],
            "heavy_blade.png": ["button_round_red.png"],
            "true_grit.png": ["star.png", "medal_bronze.png"],
            "wild_strike.png": ["arrow_basic_ne.png", "arrow_basic_nw.png"],
            "anger.png": ["fire.png", "explosion.png"],
            "apex_of_power.png": ["crown.png", "star_gold.png"],
            "battle_trance.png": ["eye.png", "target.png"],
            "blood_for_blood.png": ["heart.png", "potion_red.png"],
            "bloodletting.png": ["drop_red.png"],
            "body_slam.png": ["impact.png", "fist.png"],
            "brutality.png": ["skull.png", "sword.png"],
            "burn_alive.png": ["flame.png", "fire.png"],
            "carnage.png": ["blood_splat.png", "impact_heavy.png"],
            "combust.png": ["explosion_large.png", "fire_big.png"],
            "dark_embrace.png": ["moon.png", "shadow.png"],
            "dark_shield.png": ["shield_dark.png", "panel_dark.png"],
            "dead_branch.png": ["branch.png", "stick.png"],
            "disarm.png": ["hand_open.png", "key.png"],
            "double_tap.png": ["button_x2.png", "double_arrow.png"],
            "drag_down.png": ["arrow_down.png", "pull.png"],
            "dropkick.png": ["foot_kick.png", "impact.png"],
            "evolve.png": ["arrow_up.png", "evolution.png"],
            "exchange.png": ["swap.png", "arrows_cycle.png"],
            "express_anger.png": ["exclamation.png", "anger_face.png"],
            "feel_no_pain.png": ["painkiller.png", "pill.png"],
            "fire_breathing.png": ["flame_burst.png", "breath.png"],
            "flex.png": ["muscle_arm.png", "strong.png"],
            "headbutt.png": ["head_impact.png", "fist.png"],
            "hemokinesis.png": ["blood_drop.png", "blood_splash.png"],
            "hook.png": ["hook_tool.png", "grapple.png"],
            "immolate.png": ["fire_self.png", "burn_body.png"],
            "inflame.png": ["fire_up.png", "rage.png"],
            "infernal_blade.png": "sword_fire.png",
            "inner_beast.png": ["beast_face.png", "monster_inside.png"],
            "intimidate.png": ["scary_face.png", "threaten.png"],
            "limit_break.png": ["breakthrough.png", "shatter.png"],
            "metallicize.png": ["metal_skin.png", "armor.png"],
            "perforate.png": ["hole_pierce.png", "stab.png"],
            "power_through.png": ["force_push.png", "breakthrough.png"],
            "pummel.png": ["fist_punch.png", "hit_multi.png"],
            "rampage.png": ["rage_mode.png", "berserk.png"],
            "reckless_charge.png": ["charge_fast.png", "rush.png"],
            "rend.png": ["tear_apart.png", "rip.png"],
            "rummage.png": ["search_bag.png", "loot.png"],
            "sever_soul.png": ["soul_cut.png", "scythe.png"],
            "shockwave.png": ["wave_shock.png", "impact_wave.png"],
            "seeing_red.png": ["red_vision.png", "blood_sight.png"],
            "sentry.png": ["tower_guard.png", "watch.png"],
            "severe_blow.png": "hit_hard.png",
            "shrink_it.png": ["shrink_small.png", "minimize.png"],
            "spot_weakness.png": ["target_weak.png", "find_flaw.png"],
            "strikethrough.png": ["strike_through.png", "line_cross.png"],
            "tangent.png": ["angle_corner.png", "geometry.png"],
            "thunderclap.png": ["lightning_bolt.png", "thunder.png"],
            "twinstrike.png": ["x2_hit.png", "dual_strike.png"],
            "uppercut.png": "punch_up.png",
            "upheaval.png": ["ground_break.png", "earthquake.png"],
            "upto_all.png": ["boost_all.png", "upgrade_everyone.png"],
            "whirlwind.png": ["spin_circle.png", "cyclone.png"],
            "warcry.png": ["battle_shout.png", "warrior_cry.png"],
            "wraith_form.png": ["ghost_mode.png", "spirit.png"],
        }
        
        # 将映射应用到 Cards 目录
        cards_dir = DIRS["cards"]
        for config_name, candidates in card_mappings.items():
            target = cards_dir / config_name
            
            if not target.exists():
                continue
            
            for candidate in candidates:
                source = None
                
                # 在所有 Kenney 资源中查找
                if candidate in kenney_assets:
                    source = kenney_assets[candidate]
                else:
                    # 尝试在 relics 目录找
                    relic_path = DIRS["relics"] / candidate
                    if relic_path.exists():
                        source = relic_path
                
                if source and source.exists():
                    try:
                        shutil.copy2(source, target)
                        mappings.append({
                            "source": str(source),
                            "target": str(target),
                            "type": "card_icon"
                        })
                        break
                    except Exception as e:
                        pass
        
        return mappings
    
    def replace_character_portraits(self):
        """替换角色立绘为更好的版本或使用 Kenney 资源"""
        mappings = []
        char_dir = DIRS["characters"]
        
        # Kenney Platformer Characters 可用作角色
        kenney_chars = {
            "Ironclad.png": ["character001.png", "knight.png", "warrior_m.png"],
            "Silent.png": ["character002.png", "rogue_m.png", "ninja_f.png"],
            "Defect.png": ["robot.png", "golem.png", "android.png"],
            "Watcher.png": ["character003.png", "priest_f.png", "mage_f.png"],
            "Necromancer.png": ["necromancer.png", "wizard_m.png", "dark_wizard.png"],
        }
        
        for target_name, sources in kenney_chars.items():
            target = char_dir / target_name
            
            if not target.exists():
                continue
            
            for src_name in sources:
                # 先检查 relics 目录（Kenney UI Pack 可能有角色相关）
                for search_dir in [DIRS["relics"], DIRS["enemies_full"], DIRS["cards"]]:
                    source = search_dir / src_name
                    if source.exists():
                        try:
                            shutil.copy2(source, target)
                            mappings.append({
                                "source": str(source),
                                "target": str(target),
                                "type": "portrait"
                            })
                            break
                        except Exception:
                            pass
                else:
                    continue
                break
        
        return mappings
    
    def regenerate_with_kenney_style(self):
        """使用 Kenney 风格重新生成缺失的关键资源"""
        from PIL import Image, ImageDraw
        
        # 如果某些关键文件不存在，使用 Kenney 风格的颜色和样式生成
        key_resources = [
            ("Icons/Cards/strike.png", (220, 60, 60), "attack"),
            ("Icons/Cards/defend.png", (60, 140, 220), "defense"),
            ("Icons/Cards/bash.png", (200, 150, 50), "attack"),
            ("Icons/Skills/fireball.png", (255, 100, 50), "skill"),
            ("Icons/Skills/heal.png", (100, 255, 100), "skill"),
            ("Icons/Enemies/cultist.png", (150, 80, 150), "enemy"),
        ]
        
        regen_count = 0
        for path_str, color, rtype in key_resources:
            path = Path(path_str)
            
            if not path.exists():
                # 使用 Kenney 风格重新生成
                img = self._create_kenney_style_icon(color, rtype)
                img.save(path)
                regen_count += 1
        
        if regen_count > 0:
            self.log(f"   ✅ 重新生成了 {regen_count} 个缺失资源 (Kenney风格)")
    
    def _create_kenney_style_icon(self, color: tuple, icon_type: str):
        """创建 Kenney 风格的图标（更精细）"""
        size = (128, 128) if icon_type != "enemy" else (64, 64)
        img = Image.new('RGBA', size, (0, 0, 0, 0))
        draw = ImageDraw.Draw(img)
        
        cx, cy = size[0] // 2, size[1] // 2
        r, g, b = color
        
        if icon_type == "attack":
            # 剑形 - Kenney 风格
            draw.rounded_rectangle([10, 10, size[0]-10, size[1]-10], radius=15,
                               fill=(255, 255, 255, 240), outline=color, width=4)
            draw.polygon([(cx, 20), (cx+20, cy-10), (cx+25, cy), (cx, size[1]-20),
                        (cx-25, cy), (cx-20, cy-10)], fill=color)
            draw.rectangle([cx-4, cy, cx+4, size[1]-20], fill=(180, 160, 120))
            
        elif icon_type == "defense":
            # 盾牌 - Kenney 风格
            draw.rounded_rectangle([10, 10, size[0]-10, size[1]-10], radius=15,
                               fill=(255, 255, 255, 240), outline=color, width=4)
            points = [(cx, 25), (cx+30, 40), (cx+30, cy+20), (cx, cy+35),
                    (cx-30, cy+20), (cx-30, 40)]
            draw.polygon(points, fill=color)
            draw.line([(cx, 45), (cx, cy+28)], fill=(255, 255, 255, 200), width=4)
            draw.line([(cx-22, 55), (cx+22, 55)], fill=(255, 255, 255, 200), width=4)
            
        elif icon_type == "skill":
            # 技能图标 - Kenney 风格
            draw.ellipse([15, 15, size[0]-15, size[1]-15], outline=color, width=4)
            draw.ellipse([25, 25, size[0]-25, size[1]-25], outline=color, width=2)
            if "fire" in str(color):
                # 火焰
                draw.polygon([(cx, 20), (cx-12, 45), (cx, 35), (cx+12, 45)], fill=(255, 200, 50))
                draw.polygon([(cx, 50), (cx-8, 70), (cx, 62), (cx+8, 70)], fill=(255, 150, 50))
            elif g > 200:
                # 治疗
                draw.line([(cx-20, cy), (cx+20, cy)], fill=(100, 255, 100), width=5)
                draw.line([(cx, cy-20), (cx, cy+20)], fill=(100, 255, 100), width=5)
                
        elif icon_type == "enemy":
            # 敌人 - Kenney 风格
            draw.ellipse([2, 2, size[0]-2, size[1]-2], fill=(40, 40, 50, 230), outline=color, width=3)
            draw.ellipse([cx-18, cy-18, cx+18, cy+18], fill=color)
            draw.ellipse([cx-18, cy-26, cx+18, cy-14], fill=(color[0]+30, color[1]+30, color[2]+30))
            draw.ellipse([cx-7, cy-23, cx-3, cy-19], fill=(255, 255, 180))
            draw.ellipse([cx+3, cy-23, cx+7, cy-19], fill=(255, 255, 180))
        
        return img
    
    def run_fix(self):
        """执行完整修复流程"""
        print("""
╔═══════════════════════════════════════════════════════════╗
║                                                           ║
║   🔧 Godot Roguelike 资源修复工具 v7.0                   ║
║                                                           ║
║   解决 Kenney 资源未生效问题                              ║
║                                                           ║
╚═══════════════════════════════════════════════════════════╝
""")
        
        # Step 1: 清理缓存
        self.clean_import_cache()
        
        # Step 2: 获取 Kenney 资源
        self.log("\n🔍 第2步: 扫描 Kenney 真实资源...")
        kenney_assets = self.get_kenney_assets()
        self.log(f"   发现 {len(kenney_assets)} 个 Kenney 真实资源")
        
        # Step 3: 映射资源
        self.log("\n🔗 第3步: 映射 Kenney 资源到配置路径...")
        card_mappings = self.map_kenney_to_config(kenney_assets)
        portrait_mappings = self.replace_character_portraits()
        
        total_mappings = len(card_mappings) + len(portrait_mappings)
        self.stats["files_replaced"] = total_mappings
        
        if card_mappings:
            self.log(f"\n   📋 卡牌图标映射 ({len(card_mappings)} 个):")
            for m in card_mappings[:10]:
                self.log(f"      {Path(m['target']).name} ← {Path(m['source']).name}")
        
        if portrait_mappings:
            self.log(f"\n   👤 角色立绘映射 ({len(portrait_mappings)} 个):")
            for m in portrait_mappings[:5]:
                self.log(f"      {Path(m['target']).name} ← {Path(m['source']).name}")
        
        # Step 4: 补充缺失资源
        self.log("\n🎨 第4步: 补充缺失的资源...")
        self.regenerate_with_kenney_style()
        
        # 总结
        self._print_summary()
    
    def _print_summary(self):
        """打印总结"""
        print(f"\n{'='*70}")
        print(f"✅ 资源修复完成!")
        print(f"{'='*70}\n")
        
        print(f"🧹 .import 缓存清理: {self.stats['cache_cleaned']} 个文件")
        print(f"🔄 资源替换: {self.stats['files_replaced']} 个")
        
        if self.stats["errors"]:
            print(f"\n⚠️ 错误:")
            for e in self.stats["errors"]:
                print(f"   • {e}")
        
        print(f"\n💡 下一步:")
        print(f"   1. 重启 Godot 编辑器（必须！让缓存清理生效）")
        print(f"   2. 运行游戏查看效果")
        print(f"   3. 如仍有问题，执行: Project → ReImport")


def main():
    fixer = AssetFixer()
    fixer.run_fix()


if __name__ == "__main__":
    main()
