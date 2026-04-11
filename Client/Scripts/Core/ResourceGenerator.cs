using Godot;
using System;
using System.Collections.Generic;
using System.IO;

namespace RoguelikeGame.Core
{
    public partial class ResourceGenerator : SingletonBase<ResourceGenerator>
    {
        private readonly RandomNumberGenerator _rng = new();
        private const string IMAGES_PATH = "res://GameModes/base_game/Resources/Images/";
        private const string ICONS_PATH = "res://GameModes/base_game/Resources/Icons/";

        protected override void OnInitialize()
        {
            _rng.Randomize();
        }

        public void GenerateAllResources()
        {
            GD.Print("[ResourceGenerator] 开始生成所有资源...");

            GenerateCharacterPortraits();
            GenerateBackgrounds();
            GenerateUIIcons();
            GenerateEnemyImages();
            GenerateRelicImages();
            GeneratePotionImages();
            GenerateEventImages();
            GenerateCardIcons();

            GD.Print("[ResourceGenerator] 所有资源生成完成！");
        }

        private void GenerateCharacterPortraits()
        {
            GD.Print("[ResourceGenerator] 生成角色肖像...");

            var characters = new Dictionary<string, (string color, string name)>
            {
                {"Ironclad", ("#FF4444", "铁甲战士")},
                {"Silent", ("#44FF44", "静默猎人")},
                {"Defect", ("#4444FF", "缺陷机器人")},
                {"Watcher", ("#AA44AA", "守望者")},
                {"Necromancer", ("#444444", "死灵法师")},
                {"Heir", ("#FFAA44", "继承者")}
            };

            foreach (var kv in characters)
            {
                var img = GenerateCharacterPortrait(kv.Key, kv.Value.color);
                SaveImage(img, $"{IMAGES_PATH}Characters/{kv.Key}.png");
            }
        }

        private Image GenerateCharacterPortrait(string characterName, string colorHex)
        {
            int w = 200, h = 280;
            var img = Image.Create(w, h, false, Image.Format.Rgba8);

            Color themeColor;
            try { themeColor = new Color(colorHex); }
            catch { themeColor = new Color(0.5f, 0.5f, 0.5f); }

            img.Fill(themeColor.Darkened(0.6f));

            DrawCharacterBackground(img, w, h, themeColor);
            DrawCharacterBody(img, w, h, characterName, themeColor);
            DrawCharacterFrame(img, w, h, themeColor);

            return img;
        }

        private void DrawCharacterBackground(Image img, int w, int h, Color theme)
        {
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float nx = (float)x / w;
                    float ny = (float)y / h;
                    float v = Mathf.Sin(nx * 10f) * Mathf.Cos(ny * 10f) * 0.1f + 0.5f;
                    var c = theme.Darkened(0.5f + v * 0.2f);
                    img.SetPixel(x, y, c);
                }
            }
        }

        private void DrawCharacterBody(Image img, int w, int h, string name, Color theme)
        {
            int cx = w / 2;
            int baseY = h - 60;

            var dark = theme.Darkened(0.4f);
            var light = theme.Lightened(0.3f);

            DrawCircleFilled(img, cx, baseY - 120, 30, dark);
            DrawEllipseFilled(img, cx, baseY - 80, 25, 40, dark);

            switch (name)
            {
                case "Ironclad":
                    DrawRectFilled(img, cx - 30, baseY - 70, 60, 50, dark);
                    DrawRectFilled(img, cx - 25, baseY - 25, 20, 35, dark);
                    DrawRectFilled(img, cx + 5, baseY - 25, 20, 35, dark);
                    DrawLineThick(img, cx - 15, baseY - 50, cx - 40, baseY - 10, light, 5);
                    DrawLineThick(img, cx + 15, baseY - 50, cx + 40, baseY - 10, light, 5);
                    break;

                case "Silent":
                    DrawEllipseFilled(img, cx, baseY - 75, 18, 35, dark);
                    DrawRectFilled(img, cx - 15, baseY - 25, 12, 35, dark);
                    DrawRectFilled(img, cx + 3, baseY - 25, 12, 35, dark);
                    DrawLineThick(img, cx - 10, baseY - 60, cx - 30, baseY - 15, light, 3);
                    DrawLineThick(img, cx + 10, baseY - 60, cx + 30, baseY - 15, light, 3);
                    break;

                case "Defect":
                    DrawRectFilled(img, cx - 28, baseY - 75, 56, 55, dark);
                    DrawRectFilled(img, cx - 20, baseY - 25, 15, 35, dark);
                    DrawRectFilled(img, cx + 5, baseY - 25, 15, 35, dark);
                    DrawRectFilled(img, cx - 12, baseY - 65, 8, 8, new Color(0f, 0.8f, 1f));
                    DrawRectFilled(img, cx + 4, baseY - 65, 8, 8, new Color(0f, 0.8f, 1f));
                    DrawLineThick(img, cx, baseY - 80, cx, baseY - 95, light, 3);
                    DrawCircleFilled(img, cx, baseY - 98, 5, new Color(0f, 0.8f, 1f));
                    break;

                case "Watcher":
                    DrawTriangleFilled(img, cx, baseY - 85, cx - 22, baseY - 40, cx + 22, baseY - 40, dark);
                    DrawRectFilled(img, cx - 12, baseY - 25, 10, 35, dark);
                    DrawRectFilled(img, cx + 2, baseY - 25, 10, 35, dark);
                    DrawCircleFilled(img, cx, baseY - 105, 8, new Color(1f, 0.9f, 0.5f));
                    break;

                case "Necromancer":
                    DrawEllipseFilled(img, cx, baseY - 75, 20, 38, dark);
                    DrawRectFilled(img, cx - 18, baseY - 25, 14, 35, dark);
                    DrawRectFilled(img, cx + 4, baseY - 25, 14, 35, dark);
                    DrawCircleFilled(img, cx - 25, baseY - 90, 6, new Color(0.5f, 0f, 0.5f, 0.7f));
                    DrawCircleFilled(img, cx + 25, baseY - 90, 6, new Color(0.5f, 0f, 0.5f, 0.7f));
                    DrawCircleFilled(img, cx, baseY - 110, 5, new Color(0.3f, 0f, 0.3f, 0.8f));
                    break;

                case "Heir":
                    DrawEllipseFilled(img, cx, baseY - 75, 22, 38, dark);
                    DrawRectFilled(img, cx - 16, baseY - 25, 12, 35, dark);
                    DrawRectFilled(img, cx + 4, baseY - 25, 12, 35, dark);
                    DrawCircleFilled(img, cx, baseY - 100, 10, new Color(1f, 0.85f, 0.3f));
                    DrawSmallStar(img, cx, baseY - 100, 8, new Color(1f, 0.9f, 0.5f));
                    break;
            }
        }

        private void DrawCharacterFrame(Image img, int w, int h, Color theme)
        {
            var frameColor = theme.Lightened(0.4f);
            frameColor.A = 0.8f;

            for (int x = 0; x < w; x++)
            {
                img.SetPixel(x, 0, frameColor);
                img.SetPixel(x, 1, frameColor);
                img.SetPixel(x, h - 1, frameColor);
                img.SetPixel(x, h - 2, frameColor);
            }

            for (int y = 0; y < h; y++)
            {
                img.SetPixel(0, y, frameColor);
                img.SetPixel(1, y, frameColor);
                img.SetPixel(w - 1, y, frameColor);
                img.SetPixel(w - 2, y, frameColor);
            }
        }

        private void GenerateBackgrounds()
        {
            GD.Print("[ResourceGenerator] 生成背景图像...");

            var backgrounds = new Dictionary<string, Color>
            {
                {"glory", new Color(0.6f, 0.5f, 0.3f)},
                {"hive", new Color(0.5f, 0.4f, 0.2f)},
                {"overgrowth", new Color(0.3f, 0.5f, 0.3f)},
                {"underdocks", new Color(0.2f, 0.3f, 0.4f)}
            };

            foreach (var kv in backgrounds)
            {
                var img = GenerateBackground(kv.Key, kv.Value);
                SaveImage(img, $"{IMAGES_PATH}Backgrounds/{kv.Key}.png");
            }
        }

        private Image GenerateBackground(string name, Color baseColor)
        {
            int w = 1280, h = 720;
            var img = Image.Create(w, h, false, Image.Format.Rgba8);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float nx = (float)x / w;
                    float ny = (float)y / h;

                    float noise = Mathf.Sin(nx * 20f + ny * 15f) * 0.05f +
                                  Mathf.Sin(nx * 35f - ny * 25f) * 0.03f +
                                  Mathf.Sin(ny * 30f) * 0.02f;

                    var c = baseColor.Darkened(ny * 0.3f);
                    c = c.Lightened(noise);

                    img.SetPixel(x, y, c);
                }
            }

            DrawBackgroundDetails(img, w, h, name, baseColor);

            return img;
        }

        private void DrawBackgroundDetails(Image img, int w, int h, string name, Color baseColor)
        {
            _rng.Seed = (ulong)name.GetHashCode();

            for (int i = 0; i < 100; i++)
            {
                int x = _rng.RandiRange(0, w - 1);
                int y = _rng.RandiRange(0, h - 1);
                int size = _rng.RandiRange(1, 4);
                var c = baseColor.Lightened(_rng.RandfRange(0.1f, 0.3f));
                c.A = _rng.RandfRange(0.1f, 0.3f);

                DrawDot(img, x, y, size, c);
            }

            int floorY = (int)(h * 0.75f);
            var floorColor = baseColor.Darkened(0.3f);
            floorColor.A = 0.5f;

            for (int y = floorY; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var existing = img.GetPixel(x, y);
                    float blend = (float)(y - floorY) / (h - floorY);
                    img.SetPixel(x, y, existing.Lerp(floorColor, blend * 0.5f));
                }
            }
        }

        private void GenerateUIIcons()
        {
            GD.Print("[ResourceGenerator] 生成UI图标...");

            GenerateRestIcons();
            GenerateAchievementIcons();
            GenerateSkillIcons();
            GenerateItemIcons();
            GenerateServiceIcons();
        }

        private void GenerateRestIcons()
        {
            var icons = new Dictionary<string, Color>
            {
                {"heal", new Color(0.3f, 1f, 0.4f)},
                {"upgrade", new Color(1f, 0.85f, 0.3f)},
                {"recall", new Color(0.5f, 0.5f, 1f)},
                {"smith", new Color(0.8f, 0.5f, 0.3f)},
                {"default", new Color(0.5f, 0.5f, 0.5f)}
            };

            foreach (var kv in icons)
            {
                var img = GenerateRestIcon(kv.Key, kv.Value);
                SaveImage(img, $"{ICONS_PATH}Rest/{kv.Key}.png");
            }
        }

        private Image GenerateRestIcon(string type, Color color)
        {
            int size = 48;
            var img = Image.Create(size, size, false, Image.Format.Rgba8);
            img.Fill(new Color(0, 0, 0, 0));

            int c = size / 2;

            switch (type)
            {
                case "heal":
                    DrawHeartIcon(img, size, color);
                    break;
                case "upgrade":
                    DrawArrowUpIcon(img, size, color);
                    break;
                case "recall":
                    DrawRecallIcon(img, size, color);
                    break;
                case "smith":
                    DrawHammerIcon(img, size, color);
                    break;
                default:
                    DrawCircleFilled(img, c, c, size / 3, color);
                    break;
            }

            return img;
        }

        private void DrawHeartIcon(Image img, int size, Color color)
        {
            int c = size / 2;
            DrawCircleFilled(img, c - 6, c - 3, 7, color);
            DrawCircleFilled(img, c + 6, c - 3, 7, color);
            DrawTriangleFilled(img, c - 12, c + 1, c + 12, c + 1, c, c + 16, color);
        }

        private void DrawArrowUpIcon(Image img, int size, Color color)
        {
            int c = size / 2;
            DrawLineThick(img, c, size - 8, c, 8, color, 4);
            DrawLineThick(img, c, 8, c - 8, 18, color, 4);
            DrawLineThick(img, c, 8, c + 8, 18, color, 4);
        }

        private void DrawRecallIcon(Image img, int size, Color color)
        {
            int c = size / 2;
            DrawArc(img, c, c, 12, 0, Mathf.Pi * 1.5f, color, 3);
            DrawLineThick(img, c + 12, c, c + 12, c - 8, color, 3);
            DrawLineThick(img, c + 12, c, c + 4, c, color, 3);
        }

        private void DrawHammerIcon(Image img, int size, Color color)
        {
            int c = size / 2;
            DrawRectFilled(img, c - 10, c - 15, 20, 12, color);
            DrawRectFilled(img, c - 3, c - 3, 6, 20, color.Darkened(0.2f));
        }

        private void GenerateAchievementIcons()
        {
            var icons = new Dictionary<string, Color>
            {
                {"FirstVictory", new Color(1f, 0.85f, 0.3f)},
                {"Kill100", new Color(0.8f, 0.2f, 0.2f)},
                {"AllRelics", new Color(0.6f, 0.3f, 0.8f)},
                {"NoDamage", new Color(0.3f, 0.8f, 0.3f)}
            };

            foreach (var kv in icons)
            {
                var img = GenerateAchievementIcon(kv.Key, kv.Value);
                SaveImage(img, $"{ICONS_PATH}Achievements/{kv.Key}.png");
            }
        }

        private Image GenerateAchievementIcon(string type, Color color)
        {
            int size = 64;
            var img = Image.Create(size, size, false, Image.Format.Rgba8);
            img.Fill(new Color(0, 0, 0, 0));

            int c = size / 2;

            switch (type)
            {
                case "FirstVictory":
                    DrawTrophyIcon(img, size, color);
                    break;
                case "Kill100":
                    DrawSkullIcon(img, size, color);
                    break;
                case "AllRelics":
                    DrawDiamondShape(img, c, c, 20, color);
                    break;
                case "NoDamage":
                    DrawShieldIcon(img, size, color);
                    break;
                default:
                    DrawCircleFilled(img, c, c, size / 3, color);
                    break;
            }

            return img;
        }

        private void DrawTrophyIcon(Image img, int size, Color color)
        {
            int c = size / 2;
            DrawRectFilled(img, c - 12, c - 15, 24, 20, color);
            DrawRectFilled(img, c - 5, c + 5, 10, 8, color.Darkened(0.2f));
            DrawRectFilled(img, c - 10, c + 13, 20, 4, color.Darkened(0.3f));
            DrawRectFilled(img, c - 18, c - 12, 6, 12, color.Lightened(0.2f));
            DrawRectFilled(img, c + 12, c - 12, 6, 12, color.Lightened(0.2f));
        }

        private void DrawSkullIcon(Image img, int size, Color color)
        {
            int c = size / 2;
            DrawEllipseFilled(img, c, c - 5, 15, 18, color);
            DrawRectFilled(img, c - 10, c + 8, 20, 10, color);
            DrawCircleFilled(img, c - 6, c - 8, 4, new Color(0.1f, 0.1f, 0.1f));
            DrawCircleFilled(img, c + 6, c - 8, 4, new Color(0.1f, 0.1f, 0.1f));
        }

        private void DrawShieldIcon(Image img, int size, Color color)
        {
            int c = size / 2;
            DrawLineThick(img, c - 15, c - 15, c + 15, c - 15, color, 4);
            DrawLineThick(img, c - 15, c - 15, c - 15, c, color, 3);
            DrawLineThick(img, c + 15, c - 15, c + 15, c, color, 3);
            DrawLineThick(img, c - 15, c, c, c + 18, color, 4);
            DrawLineThick(img, c + 15, c, c, c + 18, color, 4);
        }

        private void GenerateSkillIcons()
        {
            var icons = new Dictionary<string, Color>
            {
                {"fireball", new Color(1f, 0.4f, 0.1f)},
                {"heal", new Color(0.3f, 1f, 0.4f)},
                {"dash", new Color(0.3f, 0.6f, 1f)},
                {"iron_skin", new Color(0.6f, 0.6f, 0.6f)}
            };

            foreach (var kv in icons)
            {
                var img = GenerateSkillIcon(kv.Key, kv.Value);
                SaveImage(img, $"{ICONS_PATH}Skills/{kv.Key}.png");
            }
        }

        private Image GenerateSkillIcon(string type, Color color)
        {
            int size = 48;
            var img = Image.Create(size, size, false, Image.Format.Rgba8);
            img.Fill(new Color(0, 0, 0, 0));

            int c = size / 2;

            switch (type)
            {
                case "fireball":
                    DrawCircleFilled(img, c, c, 15, color);
                    DrawCircleFilled(img, c - 5, c - 5, 6, color.Lightened(0.4f));
                    break;
                case "heal":
                    DrawHeartIcon(img, size, color);
                    break;
                case "dash":
                    DrawArrowRightIcon(img, size, color);
                    break;
                case "iron_skin":
                    DrawShieldIcon(img, size, color);
                    break;
                default:
                    DrawCircleFilled(img, c, c, size / 3, color);
                    break;
            }

            return img;
        }

        private void DrawArrowRightIcon(Image img, int size, Color color)
        {
            int c = size / 2;
            DrawLineThick(img, 8, c, size - 8, c, color, 4);
            DrawLineThick(img, size - 12, c, size - 20, c - 8, color, 3);
            DrawLineThick(img, size - 12, c, size - 20, c + 8, color, 3);
        }

        private void GenerateItemIcons()
        {
            var icons = new Dictionary<string, Color>
            {
                {"health_potion_small", new Color(0.9f, 0.2f, 0.3f)},
                {"health_potion_large", new Color(0.9f, 0.2f, 0.3f)},
                {"iron_sword", new Color(0.7f, 0.7f, 0.75f)},
                {"steel_armor", new Color(0.5f, 0.5f, 0.6f)}
            };

            foreach (var kv in icons)
            {
                var img = GenerateItemIcon(kv.Key, kv.Value);
                SaveImage(img, $"{ICONS_PATH}Items/{kv.Key}.png");
            }
        }

        private Image GenerateItemIcon(string type, Color color)
        {
            int size = 48;
            var img = Image.Create(size, size, false, Image.Format.Rgba8);
            img.Fill(new Color(0, 0, 0, 0));

            int c = size / 2;

            switch (type)
            {
                case "health_potion_small":
                    DrawBottleShape(img, c, c + 2, color, 8);
                    break;
                case "health_potion_large":
                    DrawBottleShape(img, c, c + 2, color, 12);
                    break;
                case "iron_sword":
                    DrawSwordIcon(img, size, color);
                    break;
                case "steel_armor":
                    DrawArmorIcon(img, size, color);
                    break;
                default:
                    DrawCircleFilled(img, c, c, size / 3, color);
                    break;
            }

            return img;
        }

        private void DrawBottleShape(Image img, int cx, int cy, Color liquidColor, int size)
        {
            DrawRectFilled(img, cx - 3, cy - size - 4, 6, 5, new Color(0.7f, 0.7f, 0.75f));
            DrawEllipseFilled(img, cx, cy, size, size + 4, new Color(0.7f, 0.7f, 0.75f));
            DrawEllipseFilled(img, cx, cy + 3, size - 3, size, liquidColor);
            DrawCircleFilled(img, cx, cy - size, 3, new Color(0.8f, 0.85f, 0.9f));
        }

        private void DrawArmorIcon(Image img, int size, Color color)
        {
            int c = size / 2;
            DrawEllipseFilled(img, c, c, 14, 18, color);
            DrawEllipseFilled(img, c, c, 10, 14, color.Lightened(0.2f));
            DrawRectFilled(img, c - 3, c - 8, 6, 16, color.Darkened(0.2f));
        }

        private void GenerateServiceIcons()
        {
            var img = Image.Create(48, 48, false, Image.Format.Rgba8);
            img.Fill(new Color(0, 0, 0, 0));
            DrawCircleFilled(img, 24, 24, 16, new Color(0.5f, 0.5f, 0.5f));
            SaveImage(img, $"{ICONS_PATH}Services/default.png");
        }

        private void GenerateEnemyImages()
        {
            GD.Print("[ResourceGenerator] 生成敌人图像...");

            var enemies = new Dictionary<string, (Color color, string type)>
            {
                {"Cultist", (new Color(0.6f, 0.3f, 0.6f), "humanoid")},
                {"JawWorm", (new Color(0.5f, 0.4f, 0.3f), "beast")},
                {"Lagavulin", (new Color(0.4f, 0.5f, 0.6f), "construct")},
                {"TheGuardian", (new Color(0.7f, 0.3f, 0.3f), "boss")}
            };

            foreach (var kv in enemies)
            {
                var img = GenerateEnemyImage(kv.Key, kv.Value.color, kv.Value.type);
                SaveImage(img, $"{IMAGES_PATH}Enemies/{kv.Key}.png");

                var icon = GenerateEnemyIcon(kv.Key, kv.Value.color);
                SaveImage(icon, $"{ICONS_PATH}Enemies/{kv.Key.ToLower()}.png");
            }
        }

        private Image GenerateEnemyImage(string name, Color color, string type)
        {
            int w = 150, h = 200;
            var img = Image.Create(w, h, false, Image.Format.Rgba8);
            img.Fill(color.Darkened(0.5f));

            DrawEnemyBackground(img, w, h, color);
            DrawEnemyBody(img, w, h, type, color);
            DrawEnemyFrame(img, w, h, color);

            return img;
        }

        private void DrawEnemyBackground(Image img, int w, int h, Color color)
        {
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float noise = Mathf.Sin((float)x / 10f) * Mathf.Cos((float)y / 10f) * 0.05f;
                    var c = color.Darkened(0.4f + noise);
                    img.SetPixel(x, y, c);
                }
            }
        }

        private void DrawEnemyBody(Image img, int w, int h, string type, Color color)
        {
            int cx = w / 2;
            int cy = h / 2;

            var dark = color.Darkened(0.3f);
            var light = color.Lightened(0.2f);

            switch (type)
            {
                case "humanoid":
                    DrawCircleFilled(img, cx, cy - 30, 25, dark);
                    DrawEllipseFilled(img, cx, cy + 20, 30, 45, dark);
                    DrawLineThick(img, cx - 25, cy + 10, cx - 45, cy + 50, light, 5);
                    DrawLineThick(img, cx + 25, cy + 10, cx + 45, cy + 50, light, 5);
                    break;

                case "beast":
                    DrawEllipseFilled(img, cx, cy, 50, 35, dark);
                    DrawCircleFilled(img, cx - 35, cy - 10, 12, dark);
                    DrawCircleFilled(img, cx + 35, cy - 10, 12, dark);
                    DrawCircleFilled(img, cx - 35, cy - 12, 5, new Color(1f, 0.3f, 0.3f));
                    DrawCircleFilled(img, cx + 35, cy - 12, 5, new Color(1f, 0.3f, 0.3f));
                    break;

                case "construct":
                    DrawRectFilled(img, cx - 35, cy - 40, 70, 80, dark);
                    DrawRectFilled(img, cx - 25, cy - 30, 20, 20, new Color(0.2f, 0.8f, 1f));
                    DrawRectFilled(img, cx + 5, cy - 30, 20, 20, new Color(0.2f, 0.8f, 1f));
                    DrawRectFilled(img, cx - 15, cy + 10, 30, 20, light);
                    break;

                case "boss":
                    DrawCircleFilled(img, cx, cy - 20, 40, dark);
                    DrawEllipseFilled(img, cx, cy + 40, 45, 50, dark);
                    DrawCircleFilled(img, cx - 18, cy - 25, 8, new Color(1f, 0.5f, 0f));
                    DrawCircleFilled(img, cx + 18, cy - 25, 8, new Color(1f, 0.5f, 0f));
                    DrawLineThick(img, cx - 35, cy - 50, cx - 45, cy - 70, light, 6);
                    DrawLineThick(img, cx + 35, cy - 50, cx + 45, cy - 70, light, 6);
                    break;
            }
        }

        private void DrawEnemyFrame(Image img, int w, int h, Color color)
        {
            var frameColor = color.Lightened(0.3f);
            frameColor.A = 0.6f;

            for (int x = 0; x < w; x++)
            {
                img.SetPixel(x, 0, frameColor);
                img.SetPixel(x, h - 1, frameColor);
            }

            for (int y = 0; y < h; y++)
            {
                img.SetPixel(0, y, frameColor);
                img.SetPixel(w - 1, y, frameColor);
            }
        }

        private Image GenerateEnemyIcon(string name, Color color)
        {
            int size = 64;
            var img = Image.Create(size, size, false, Image.Format.Rgba8);
            img.Fill(new Color(0, 0, 0, 0));

            DrawCircleFilled(img, size / 2, size / 2, size / 2 - 4, color.Darkened(0.3f));
            DrawCircleFilled(img, size / 2, size / 2, size / 2 - 8, color);
            DrawCircleFilled(img, size / 2 - 6, size / 2 - 6, 4, color.Lightened(0.4f));

            return img;
        }

        private void GenerateRelicImages()
        {
            GD.Print("[ResourceGenerator] 生成遗物图像...");

            var relics = new List<(string id, Color color, int shape)>
            {
                ("BurningBlood", new Color(0.9f, 0.3f, 0.2f), 0),
                ("Anchor", new Color(0.5f, 0.5f, 0.6f), 1),
                ("Lantern", new Color(1f, 0.9f, 0.5f), 2),
                ("IceCream", new Color(0.7f, 0.9f, 1f), 3)
            };

            for (int i = 0; i < 20; i++)
            {
                relics.Add(($"Relic_{i}", new Color(_rng.Randf(), _rng.Randf(), _rng.Randf()), _rng.RandiRange(0, 5)));
            }

            foreach (var (id, color, shape) in relics)
            {
                var img = GenerateRelicImage(color, shape);
                SaveImage(img, $"{IMAGES_PATH}Relics/{id}.png");

                var icon = GenerateRelicIcon(color, shape);
                SaveImage(icon, $"{ICONS_PATH}Relics/{id.ToLower()}.png");
            }
        }

        private Image GenerateRelicImage(Color color, int shapeType)
        {
            int size = 80;
            var img = Image.Create(size, size, false, Image.Format.Rgba8);
            img.Fill(new Color(0, 0, 0, 0));

            int c = size / 2;

            DrawCircleFilled(img, c, c, size / 2 - 3, color.Darkened(0.3f));
            DrawCircleFilled(img, c, c, size / 2 - 6, color);

            switch (shapeType)
            {
                case 0:
                    DrawDiamondShape(img, c, c, 22, Colors.White);
                    break;
                case 1:
                    DrawSmallStar(img, c, c, 20, Colors.White);
                    break;
                case 2:
                    DrawEyeShape(img, c, c, 18, Colors.White);
                    break;
                case 3:
                    DrawCrossShape(img, c, c, 20, Colors.White);
                    break;
                case 4:
                    DrawMoonShape(img, c, c, 20, Colors.White);
                    break;
                default:
                    DrawRingShape(img, c, c, 16, Colors.White);
                    break;
            }

            return img;
        }

        private Image GenerateRelicIcon(Color color, int shapeType)
        {
            int size = 48;
            var img = Image.Create(size, size, false, Image.Format.Rgba8);
            img.Fill(new Color(0, 0, 0, 0));

            int c = size / 2;

            DrawCircleFilled(img, c, c, size / 2 - 3, color.Darkened(0.3f));
            DrawCircleFilled(img, c, c, size / 2 - 6, color);

            return img;
        }

        private void GeneratePotionImages()
        {
            GD.Print("[ResourceGenerator] 生成药水图像...");

            var potions = new List<(string id, Color color)>
            {
                ("HealthPotion", new Color(0.9f, 0.2f, 0.3f)),
                ("StrengthPotion", new Color(1f, 0.6f, 0.2f)),
                ("BlockPotion", new Color(0.2f, 0.4f, 0.9f)),
                ("FirePotion", new Color(1f, 0.4f, 0.1f)),
                ("EnergyPotion", new Color(0.3f, 0.3f, 1f))
            };

            for (int i = 0; i < 15; i++)
            {
                potions.Add(($"Potion_{i}", new Color(_rng.Randf(), _rng.Randf(), _rng.Randf())));
            }

            foreach (var (id, color) in potions)
            {
                var img = GeneratePotionImage(color);
                SaveImage(img, $"{IMAGES_PATH}Potions/{id}.png");
            }
        }

        private Image GeneratePotionImage(Color liquidColor)
        {
            int size = 56;
            var img = Image.Create(size, size, false, Image.Format.Rgba8);
            img.Fill(new Color(0, 0, 0, 0));

            DrawBottleShape(img, size / 2, size / 2 + 4, liquidColor, 10);

            return img;
        }

        private void GenerateEventImages()
        {
            GD.Print("[ResourceGenerator] 生成事件图像...");

            var events = new List<(string id, Color color)>
            {
                ("BigFish", new Color(0.3f, 0.5f, 0.7f)),
                ("ShiningLight", new Color(1f, 0.95f, 0.8f)),
                ("CursedTome", new Color(0.4f, 0.2f, 0.4f))
            };

            for (int i = 0; i < 10; i++)
            {
                events.Add(($"Event_{i}", new Color(_rng.Randf() * 0.5f + 0.25f, _rng.Randf() * 0.5f + 0.25f, _rng.Randf() * 0.5f + 0.25f)));
            }

            foreach (var (id, color) in events)
            {
                var img = GenerateEventImage(id, color);
                SaveImage(img, $"{IMAGES_PATH}Events/{id}.png");
            }
        }

        private Image GenerateEventImage(string name, Color baseColor)
        {
            int w = 300, h = 200;
            var img = Image.Create(w, h, false, Image.Format.Rgba8);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float noise = Mathf.Sin((float)x / 15f) * Mathf.Cos((float)y / 15f) * 0.1f;
                    var c = baseColor.Lightened(noise);
                    img.SetPixel(x, y, c);
                }
            }

            _rng.Seed = (ulong)name.GetHashCode();
            for (int i = 0; i < 30; i++)
            {
                int x = _rng.RandiRange(20, w - 20);
                int y = _rng.RandiRange(20, h - 20);
                int size = _rng.RandiRange(5, 15);
                var c = baseColor.Lightened(_rng.RandfRange(0.1f, 0.3f));
                c.A = 0.5f;
                DrawCircleFilled(img, x, y, size, c);
            }

            return img;
        }

        private void GenerateCardIcons()
        {
            GD.Print("[ResourceGenerator] 生成卡牌图标...");

            var cards = new List<(string id, Color color, int type)>
            {
                ("strike", new Color(0.8f, 0.2f, 0.2f), 0),
                ("defend", new Color(0.2f, 0.4f, 0.8f), 1),
                ("bash", new Color(0.7f, 0.3f, 0.3f), 0),
                ("cleave", new Color(0.6f, 0.3f, 0.3f), 0),
                ("iron_wave", new Color(0.5f, 0.4f, 0.5f), 0)
            };

            for (int i = 0; i < 30; i++)
            {
                cards.Add(($"card_{i}", new Color(_rng.Randf(), _rng.Randf(), _rng.Randf()), _rng.RandiRange(0, 2)));
            }

            foreach (var (id, color, type) in cards)
            {
                var img = GenerateCardIcon(color, type);
                SaveImage(img, $"{ICONS_PATH}Cards/{id}.png");
            }
        }

        private Image GenerateCardIcon(Color color, int type)
        {
            int size = 64;
            var img = Image.Create(size, size, false, Image.Format.Rgba8);
            img.Fill(new Color(0, 0, 0, 0));

            int c = size / 2;

            DrawRoundedRect(img, 4, 4, size - 8, size - 8, 6, color.Darkened(0.3f));
            DrawRoundedRect(img, 6, 6, size - 12, size - 12, 5, color);

            switch (type)
            {
                case 0:
                    DrawSwordIcon(img, size, Colors.White);
                    break;
                case 1:
                    DrawShieldIcon(img, size, Colors.White);
                    break;
                case 2:
                    DrawSmallStar(img, c, c, 15, Colors.White);
                    break;
            }

            return img;
        }

        private void SaveImage(Image img, string path)
        {
            string absolutePath = path.Replace("res://", "");
            var error = img.SavePng(absolutePath);

            if (error == Error.Ok)
            {
                GD.Print($"[ResourceGenerator] 已保存: {path}");
            }
            else
            {
                GD.PrintErr($"[ResourceGenerator] 保存失败: {path}, 错误: {error}");
            }
        }

        #region Drawing Primitives

        private void DrawRoundedRect(Image img, int x, int y, int w, int h, int radius, Color color)
        {
            for (int ry = y; ry < y + h; ry++)
            {
                for (int rx = x; rx < x + w; rx++)
                {
                    if (rx >= 0 && rx < img.GetWidth() && ry >= 0 && ry < img.GetHeight())
                    {
                        int lx = rx - x, ly = ry - y;
                        bool inCorner = (lx < radius && ly < radius) ||
                                        (lx >= w - radius && ly < radius) ||
                                        (lx < radius && ly >= h - radius) ||
                                        (lx >= w - radius && ly >= h - radius);

                        if (inCorner)
                        {
                            int cx = (lx < radius) ? radius : w - radius - 1;
                            int cy = (ly < radius) ? radius : h - radius - 1;
                            float dist = Mathf.Sqrt((lx - cx) * (lx - cx) + (ly - cy) * (ly - cy));
                            if (dist <= radius)
                                img.SetPixel(rx, ry, color);
                        }
                        else
                        {
                            img.SetPixel(rx, ry, color);
                        }
                    }
                }
            }
        }

        private void DrawRectFilled(Image img, int x, int y, int w, int h, Color color)
        {
            for (int dy = 0; dy < h; dy++)
                for (int dx = 0; dx < w; dx++)
                    if (x + dx >= 0 && x + dx < img.GetWidth() && y + dy >= 0 && y + dy < img.GetHeight())
                        img.SetPixel(x + dx, y + dy, color);
        }

        private void DrawCircleFilled(Image img, int cx, int cy, int radius, Color color)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    if (dx * dx + dy * dy <= radius * radius)
                        if (cx + dx >= 0 && cx + dx < img.GetWidth() && cy + dy >= 0 && cy + dy < img.GetHeight())
                            img.SetPixel(cx + dx, cy + dy, color);
                }
            }
        }

        private void DrawEllipseFilled(Image img, int cx, int cy, int rx, int ry, Color color)
        {
            for (int dy = -ry; dy <= ry; dy++)
            {
                for (int dx = -rx; dx <= rx; dx++)
                {
                    if ((float)(dx * dx) / (rx * rx) + (float)(dy * dy) / (ry * ry) <= 1f)
                        if (cx + dx >= 0 && cx + dx < img.GetWidth() && cy + dy >= 0 && cy + dy < img.GetHeight())
                            img.SetPixel(cx + dx, cy + dy, color);
                }
            }
        }

        private void DrawLineThick(Image img, int x0, int y0, int x1, int y1, Color color, int thickness = 2)
        {
            int steps = Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0)) * 2;
            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps;
                int x = (int)Mathf.Lerp(x0, x1, t);
                int y = (int)Mathf.Lerp(y0, y1, t);
                DrawDot(img, x, y, thickness, color);
            }
        }

        private void DrawDot(Image img, int x, int y, int size, Color color)
        {
            for (int dy = -size; dy <= size; dy++)
                for (int dx = -size; dx <= size; dx++)
                    if (dx * dx + dy * dy <= size * size)
                        if (x + dx >= 0 && x + dx < img.GetWidth() && y + dy >= 0 && y + dy < img.GetHeight())
                            img.SetPixel(x + dx, y + dy, color);
        }

        private void DrawArc(Image img, int cx, int cy, int radius, float startAngle, float endAngle, Color color, int thickness = 1)
        {
            for (float a = startAngle; a <= endAngle; a += 0.02f)
            {
                int x = cx + (int)(Mathf.Cos(a) * radius);
                int y = cy + (int)(Mathf.Sin(a) * radius);
                DrawDot(img, x, y, thickness, color);
            }
        }

        private void DrawTriangleFilled(Image img, int x0, int y0, int x1, int y1, int x2, int y2, Color color)
        {
            int minX = Mathf.Min(x0, Mathf.Min(x1, x2));
            int maxX = Mathf.Max(x0, Mathf.Max(x1, x2));
            int minY = Mathf.Min(y0, Mathf.Min(y1, y2));
            int maxY = Mathf.Max(y0, Mathf.Max(y1, y2));

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float d1 = (x1 - x0) * (y - y0) - (x - x0) * (y1 - y0);
                    float d2 = (x2 - x1) * (y - y1) - (x - x1) * (y2 - y1);
                    float d3 = (x0 - x2) * (y - y2) - (x - x2) * (y0 - y2);

                    bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
                    bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

                    if (!(hasNeg && hasPos))
                        if (x >= 0 && x < img.GetWidth() && y >= 0 && y < img.GetHeight())
                            img.SetPixel(x, y, color);
                }
            }
        }

        private void DrawSmallStar(Image img, int cx, int cy, int outerRadius, Color color)
        {
            int innerRadius = outerRadius * 2 / 5;
            for (int i = 0; i < 10; i++)
            {
                float angle1 = (i * Mathf.Pi / 5f) - Mathf.Pi / 2f;
                float angle2 = ((i + 0.5f) * Mathf.Pi / 5f) - Mathf.Pi / 2f;
                int r1 = (i % 2 == 0) ? outerRadius : innerRadius;
                int r2 = (i % 2 == 0) ? innerRadius : outerRadius;
                int x1 = cx + (int)(Mathf.Cos(angle1) * r1);
                int y1 = cy + (int)(Mathf.Sin(angle1) * r1);
                int x2 = cx + (int)(Mathf.Cos(angle2) * r2);
                int y2 = cy + (int)(Mathf.Sin(angle2) * r2);
                DrawLineThick(img, x1, y1, x2, y2, color, 2);
            }
        }

        private void DrawDiamondShape(Image img, int cx, int cy, int size, Color color)
        {
            DrawTriangleFilled(img, cx, cy - size, cx - size, cy, cx, cy + size, color);
            DrawTriangleFilled(img, cx, cy - size, cx + size, cy, cx, cy + size, color);
        }

        private void DrawEyeShape(Image img, int cx, int cy, int size, Color color)
        {
            DrawEllipseFilled(img, cx, cy, size, size / 2, color);
            DrawCircleFilled(img, cx, cy, size / 3, new Color(0.1f, 0.1f, 0.15f));
            DrawCircleFilled(img, cx - size / 6, cy - size / 6, size / 6, Colors.White);
        }

        private void DrawCrossShape(Image img, int cx, int cy, int size, Color color)
        {
            DrawLineThick(img, cx, cy - size, cx, cy + size, color, 4);
            DrawLineThick(img, cx - size, cy, cx + size, cy, color, 4);
        }

        private void DrawMoonShape(Image img, int cx, int cy, int size, Color color)
        {
            DrawCircleFilled(img, cx, cy, size, color);
            DrawCircleFilled(img, cx + size / 2, cy - size / 3, size * 3 / 4, new Color(0.08f, 0.06f, 0.12f));
        }

        private void DrawRingShape(Image img, int cx, int cy, int radius, Color color)
        {
            DrawCircleFilled(img, cx, cy, radius, color);
            DrawCircleFilled(img, cx, cy, radius * 2 / 3, new Color(0.1f, 0.08f, 0.12f));
        }

        private void DrawSwordIcon(Image img, int size, Color color)
        {
            int c = size / 2;
            DrawLineThick(img, c - 8, c + 14, c + 3, c - 12, color, 3);
            DrawLineThick(img, c + 3, c - 12, c + 10, c - 6, color, 3);
            DrawLineThick(img, c - 6, c + 14, c + 6, c + 14, color, 4);
            DrawLineThick(img, c - 8, c + 18, c - 4, c + 22, color, 2);
            DrawLineThick(img, c + 4, c + 18, c + 8, c + 22, color, 2);
        }

        #endregion
    }
}
