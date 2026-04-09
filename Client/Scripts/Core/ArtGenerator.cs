using Godot;
using System;
using System.Collections.Generic;
using RoguelikeGame.Core;
using RoguelikeGame.Database;

namespace RoguelikeGame.Core
{
    public partial class ArtGenerator : SingletonBase<ArtGenerator>
    {
        private readonly Dictionary<string, ImageTexture> _cache = new();
        private readonly RandomNumberGenerator _rng = new();

        public static readonly Color AttackColor = new("#CC3333");
        public static readonly Color SkillColor = new("#3366CC");
        public static readonly Color PowerColor = new("#CCAA33");
        public static readonly Color StatusColor = new("#888888");
        public static readonly Color CurseColor = new("#663366");

        public static readonly Color BasicBorder = new("#888888");
        public static readonly Color CommonBorder = new("#66AA66");
        public static readonly Color UncommonBorder = new("#4488CC");
        public static readonly Color RareBorder = new("#AA66CC");
        public static readonly Color SpecialBorder = new("#FFAA00");

        private static readonly Dictionary<CardType, Color> TypeColors = new()
        {
            { CardType.Attack, AttackColor },
            { CardType.Skill, SkillColor },
            { CardType.Power, PowerColor },
            { CardType.Status, StatusColor },
            { CardType.Curse, CurseColor }
        };

        private static readonly Dictionary<CardRarity, Color> RarityBorders = new()
        {
            { CardRarity.Basic, BasicBorder },
            { CardRarity.Common, CommonBorder },
            { CardRarity.Uncommon, UncommonBorder },
            { CardRarity.Rare, RareBorder },
            { CardRarity.Special, SpecialBorder }
        };

        protected override void OnInitialize()
        {
            _rng.Randomize();
            GD.Print("[ArtGenerator] Initialized - Procedural Art System Ready");
        }

        public ImageTexture GenerateCardImage(string cardId, CardType type, CardRarity rarity)
        {
            var cacheKey = $"card_{cardId}";
            if (_cache.TryGetValue(cacheKey, out var cached))
                return cached;

            const int w = 250, h = 350;
            var img = Image.Create(w, h, false, Image.Format.Rgba8);
            img.Fill(new Color(0.12f, 0.1f, 0.15f, 1f));

            DrawCardBackground(img, w, h, type, rarity);
            DrawCardSymbol(img, w, h, type);
            DrawCardPattern(img, w, h, type, cardId);
            DrawCardGlow(img, w, h, type);

            var tex = ImageTexture.CreateFromImage(img);
            _cache[cacheKey] = tex;
            return tex;
        }

        private void DrawCardBackground(Image img, int w, int h, CardType type, CardRarity rarity)
        {
            var bgColor = TypeColors.TryGetValue(type, out var tc) ? tc : new Color(0.3f, 0.3f, 0.3f);
            var borderColor = RarityBorders.TryGetValue(rarity, out var rc) ? rc : BasicBorder;

            for (int y = 4; y < h - 4; y++)
            {
                for (int x = 4; x < w - 4; x++)
                {
                    float edgeDist = Math.Min(Math.Min(x, w - 1 - x), Math.Min(y, h - 1 - y));
                    if (edgeDist < 4f)
                    {
                        var t = edgeDist / 4f;
                        var c = bgColor.Lerp(borderColor, 1f - t);
                        img.SetPixel(x, y, c);
                    }
                    else
                    {
                        float nx = (float)x / w;
                        float ny = (float)y / h;
                        float noise = Mathf.Sin(nx * 20f) * Mathf.Cos(ny * 20f) * 0.03f;
                        var c = bgColor + new Color(noise, noise, noise * 0.5f);
                        float centerDist = Mathf.Sqrt(Mathf.Pow((float)x / w - 0.5f, 2) + Mathf.Pow((float)y / h - 0.5f, 2));
                        c = c.Lerp(bgColor * 0.6f, centerDist * 0.5f);
                        img.SetPixel(x, y, c);
                    }
                }
            }

            DrawRoundedRect(img, 8, 8, w - 16, h - 16, 6, borderColor.Darkened(0.2f));
            DrawRoundedRect(img, 12, 12, w - 24, h - 24, 4, borderColor.Lightened(0.2f));
        }

        private void DrawCardSymbol(Image img, int w, int h, CardType type)
        {
            int cx = w / 2;
            int cy = h / 2 - 20;
            int size = 50;
            Color symColor = Colors.White;

            switch (type)
            {
                case CardType.Attack:
                    DrawSwordSymbol(img, cx, cy, size, symColor);
                    break;
                case CardType.Skill:
                    DrawShieldSymbol(img, cx, cy, size, symColor);
                    break;
                case CardType.Power:
                    DrawStarSymbol(img, cx, cy, size, symColor);
                    break;
                case CardType.Status:
                    DrawArrowSymbol(img, cx, cy, size, symColor);
                    break;
                case CardType.Curse:
                    DrawSkullSymbol(img, cx, cy, size, symColor);
                    break;
            }
        }

        private void DrawCardPattern(Image img, int w, int h, CardType type, string seed)
        {
            _rng.Seed = (ulong)(seed.GetHashCode() & 0x7FFFFFFF);
            var patternColor = new Color(1f, 1f, 1f, 0.04f);

            for (int i = 0; i < 30; i++)
            {
                int px = _rng.RandiRange(20, w - 20);
                int py = _rng.RandiRange(20, h - 20);
                int ps = _rng.RandiRange(2, 6);

                switch (type)
                {
                    case CardType.Attack:
                        DrawLineThick(img, px, py, px + ps * 3, py + ps, patternColor);
                        break;
                    case CardType.Skill:
                        DrawCircleFilled(img, px, py, ps, patternColor);
                        break;
                    case CardType.Power:
                        DrawSmallStar(img, px, py, ps, patternColor);
                        break;
                    default:
                        DrawDot(img, px, py, ps, patternColor);
                        break;
                }
            }
        }

        private void DrawCardGlow(Image img, int w, int h, CardType type)
        {
            var glowColor = TypeColors.TryGetValue(type, out var c) ? c : Colors.White;
            glowColor.A = 0.08f;

            int cx = w / 2;
            int cy = h / 2 - 20;
            int radius = 70;

            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist <= radius && dist > radius * 0.5f)
                    {
                        int px = cx + dx;
                        int py = cy + dy;
                        if (px > 0 && px < w && py > 0 && py < h)
                        {
                            float alpha = (1f - dist / radius) * glowColor.A;
                            var existing = img.GetPixel(px, py);
                            img.SetPixel(px, py, existing.Lerp(glowColor, alpha));
                        }
                    }
                }
            }
        }

        public ImageTexture GenerateCharacterPortrait(string characterId, string className, string colorHex)
        {
            var cacheKey = $"portrait_{characterId}";
            if (_cache.TryGetValue(cacheKey, out var cached))
                return cached;

            const int w = 200, h = 280;
            var img = Image.Create(w, h, false, Image.Format.Rgba8);

            Color bgColor;
            try { bgColor = new Color(colorHex); } catch { bgColor = new Color(0.3f, 0.3f, 0.4f); }
            img.Fill(bgColor.Darkened(0.3f));

            DrawCharacterSilhouette(img, w, h, className, bgColor);
            DrawCharacterFrame(img, w, h, bgColor);
            DrawCharacterDetails(img, w, h, className, bgColor);

            var tex = ImageTexture.CreateFromImage(img);
            _cache[cacheKey] = tex;
            return tex;
        }

        private void DrawCharacterSilhouette(Image img, int w, int h, string className, Color themeColor)
        {
            int cx = w / 2;
            int baseY = h - 40;

            Color darkColor = themeColor.Darkened(0.5f);
            Color lightColor = themeColor.Lightened(0.2f);

            DrawCircleFilled(img, cx, baseY - 140, 28, darkColor);
            DrawEllipseFilled(img, cx, baseY - 105, 22, 35, darkColor);

            switch (className.ToLower())
            {
                case "ironclad":
                    DrawBodyBroad(img, cx, baseY, darkColor, lightColor);
                    break;
                case "silent":
                    DrawBodySlim(img, cx, baseY, darkColor, lightColor);
                    break;
                case "defect":
                    DrawBodyRobot(img, cx, baseY, darkColor, lightColor);
                    break;
                case "watcher":
                    drawBodyMystic(img, cx, baseY, darkColor, lightColor);
                    break;
                default:
                    DrawBodyGeneric(img, cx, baseY, darkColor, lightColor);
                    break;
            }
        }

        private void DrawBodyBroad(Image img, int cx, int baseY, Color dark, Color light)
        {
            DrawEllipseFilled(img, cx, baseY - 65, 30, 45, dark);
            DrawRectFilled(img, cx - 25, baseY - 100, 50, 60, dark);
            DrawRectFilled(img, cx - 22, baseY - 42, 18, 45, dark);
            DrawRectFilled(img, cx + 4, baseY - 42, 18, 45, dark);
            DrawCircleFilled(img, cx - 18, baseY + 5, 10, light);
            DrawCircleFilled(img, cx + 18, baseY + 5, 10, light);
            DrawLineThick(img, cx - 8, baseY - 80, cx - 35, baseY - 30, light, 4);
            DrawLineThick(img, cx + 8, baseY - 80, cx + 35, baseY - 30, light, 4);
        }

        private void DrawBodySlim(Image img, int cx, int baseY, Color dark, Color light)
        {
            DrawEllipseFilled(img, cx, baseY - 70, 20, 40, dark);
            DrawRectFilled(img, cx - 14, baseY - 102, 28, 55, dark);
            DrawRectFilled(img, cx - 11, baseY - 48, 10, 48, dark);
            DrawRectFilled(img, cx + 1, baseY - 48, 10, 48, dark);
            DrawCircleFilled(img, cx - 7, baseY + 2, 7, light);
            DrawCircleFilled(img, cx + 7, baseY + 2, 7, light);
            DrawLineThick(img, cx - 6, baseY - 85, cx - 28, baseY - 25, light, 3);
            DrawLineThick(img, cx + 6, baseY - 85, cx + 28, baseY - 25, light, 3);
        }

        private void DrawBodyRobot(Image img, int cx, int baseY, Color dark, Color light)
        {
            DrawRectFilled(img, cx - 26, baseY - 108, 52, 68, dark);
            DrawRectFilled(img, cx - 18, baseY - 42, 14, 46, dark);
            DrawRectFilled(img, cx + 4, baseY - 42, 14, 46, dark);
            DrawRectFilled(img, cx - 12, baseY + 6, 10, 8, light);
            DrawRectFilled(img, cx + 2, baseY + 6, 10, 8, light);
            DrawRectFilled(img, cx - 20, baseY - 95, 8, 8, new Color(0f, 0.8f, 1f));
            DrawRectFilled(img, cx + 12, baseY - 95, 8, 8, new Color(0f, 0.8f, 1f));
            DrawRectFilled(img, cx - 8, baseY - 75, 16, 4, light);
            DrawLineThick(img, cx - 12, baseY - 75, cx - 32, baseY - 20, light, 4);
            DrawLineThick(img, cx + 12, baseY - 75, cx + 32, baseY - 20, light, 4);
            DrawAntenna(img, cx, baseY - 110, light);
        }

        private void drawBodyMystic(Image img, int cx, int baseY, Color dark, Color light)
        {
            DrawEllipseFilled(img, cx, baseY - 68, 22, 42, dark);
            DrawTriangleFilled(img, cx, baseY - 115, cx - 20, baseY - 60, cx + 20, baseY - 60, dark);
            DrawRectFilled(img, cx - 12, baseY - 58, 10, 50, dark);
            DrawRectFilled(img, cx + 2, baseY - 58, 10, 50, dark);
            DrawCircleFilled(img, cx - 8, baseY + 0, 7, light);
            DrawCircleFilled(img, cx + 8, baseY + 0, 7, light);
            DrawOrbFloat(img, cx, baseY - 135, 10, new Color(1f, 0.9f, 0.5f));
            DrawOrbFloat(img, cx - 25, baseY - 120, 6, new Color(0.5f, 0.5f, 1f));
            DrawOrbFloat(img, cx + 25, baseY - 120, 6, new Color(0.5f, 1f, 0.5f));
        }

        private void DrawBodyGeneric(Image img, int cx, int baseY, Color dark, Color light)
        {
            DrawEllipseFilled(img, cx, baseY - 68, 24, 44, dark);
            DrawRectFilled(img, cx - 16, baseY - 100, 32, 56, dark);
            DrawRectFilled(img, cx - 13, baseY - 46, 12, 48, dark);
            DrawRectFilled(img, cx + 1, baseY - 46, 12, 48, dark);
            DrawCircleFilled(img, cx - 8, baseY + 4, 8, light);
            DrawCircleFilled(img, cx + 8, baseY + 4, 8, light);
        }

        private void DrawCharacterFrame(Image img, int w, int h, Color themeColor)
        {
            DrawRoundedRect(img, 2, 2, w - 4, h - 4, 8, themeColor.Lightened(0.3f));
            DrawRoundedRect(img, 5, 5, w - 10, h - 10, 6, themeColor.Darkened(0.3f));

            for (int i = 0; i < w; i++)
            {
                float gradient = (float)i / w;
                var c = themeColor.Lightened(0.2f).Lerp(themeColor.Darkened(0.2f), gradient);
                c.A = 0.3f;
                img.SetPixel(i, 0, c);
                img.SetPixel(i, h - 1, c);
            }
        }

        private void DrawCharacterDetails(Image img, int w, int h, string className, Color themeColor)
        {
            var accentColor = themeColor.Lightened(0.5f);
            accentColor.A = 0.3f;

            int stripeY = h - 25;
            for (int x = 10; x < w - 10; x++)
            {
                for (int sy = 0; sy < 4; sy++)
                {
                    if (x + sy < w - 10 && stripeY + sy < h - 5)
                        img.SetPixel(x, stripeY + sy, accentColor);
                }
            }

            DrawSmallIcon(img, w - 20, 15, className, themeColor);
        }

        public ImageTexture GenerateIcon(string iconName, IconType type, Color? customColor = null)
        {
            var cacheKey = $"icon_{iconName}_{type}";
            if (_cache.TryGetValue(cacheKey, out var cached))
                return cached;

            const int size = 64;
            var img = Image.Create(size, size, false, Image.Format.Rgba8);
            img.Fill(new Color(0, 0, 0, 0));

            Color iconColor = customColor ?? Colors.White;

            switch (type)
            {
                case IconType.Attack:
                    DrawSwordIcon(img, size, iconColor);
                    break;
                case IconType.Defend:
                    DrawShieldIcon(img, size, iconColor);
                    break;
                case IconType.Energy:
                    DrawEnergyIcon(img, size, iconColor);
                    break;
                case IconType.Health:
                    DrawHeartIcon(img, size, iconColor);
                    break;
                case IconType.Gold:
                    DrawGoldIcon(img, size, iconColor);
                    break;
                case IconType.Potion:
                    DrawPotionIcon(img, size, iconColor);
                    break;
                case IconType.Relic:
                    DrawRelicIcon(img, size, iconColor);
                    break;
                case IconType.MapNode:
                    DrawMapNodeIcon(img, size, iconColor);
                    break;
                case IconType.Settings:
                    DrawGearIcon(img, size, iconColor);
                    break;
                case IconType.Close:
                    DrawXIcon(img, size, iconColor);
                    break;
                case IconType.ArrowRight:
                    DrawArrowIcon(img, size, iconColor);
                    break;
                case IconType.ArrowLeft:
                    DrawArrowLeftIcon(img, size, iconColor);
                    break;
                default:
                    DrawCircleFilled(img, size / 2, size / 2, size / 3, iconColor);
                    break;
            }

            var tex = ImageTexture.CreateFromImage(img);
            _cache[cacheKey] = tex;
            return tex;
        }

        public ImageTexture GenerateMapNodeImage(MapNodeType nodeType)
        {
            var cacheKey = $"mapnode_{nodeType}";
            if (_cache.TryGetValue(cacheKey, out var cached))
                return cached;

            const int size = 80;
            var img = Image.Create(size, size, false, Image.Format.Rgba8);
            img.Fill(new Color(0, 0, 0, 0));

            Color nodeColor, iconColor;
            switch (nodeType)
            {
                case MapNodeType.Monster:
                    nodeColor = new Color(0.8f, 0.2f, 0.2f); iconColor = Colors.White;
                    break;
                case MapNodeType.Elite:
                    nodeColor = new Color(1f, 0.5f, 0f); iconColor = Colors.White;
                    break;
                case MapNodeType.Boss:
                    nodeColor = new Color(0.6f, 0.05f, 0.05f); iconColor = new Color(1f, 0.8f, 0f);
                    break;
                case MapNodeType.Event:
                    nodeColor = new Color(0.2f, 0.7f, 0.3f); iconColor = Colors.White;
                    break;
                case MapNodeType.Shop:
                    nodeColor = new Color(0.2f, 0.4f, 0.9f); iconColor = new Color(1f, 0.85f, 0f);
                    break;
                case MapNodeType.RestSite:
                    nodeColor = new Color(0.2f, 0.6f, 0.3f); iconColor = Colors.White;
                    break;
                case MapNodeType.Treasure:
                    nodeColor = new Color(0.9f, 0.7f, 0.1f); iconColor = Colors.White;
                    break;
                default:
                    nodeColor = new Color(0.5f, 0.5f, 0.5f); iconColor = Colors.White;
                    break;
            }

            DrawCircleFilled(img, size / 2, size / 2, size / 2 - 2, nodeColor.Darkened(0.2f));
            DrawCircleFilled(img, size / 2, size / 2, size / 2 - 5, nodeColor);
            DrawCircleOutline(img, size / 2, size / 2, size / 2 - 3, nodeColor.Lightened(0.4f), 2);

            switch (nodeType)
            {
                case MapNodeType.Monster:
                case MapNodeType.Elite:
                case MapNodeType.Boss:
                    DrawSkullMini(img, size / 2, size / 2, 16, iconColor);
                    break;
                case MapNodeType.Event:
                    DrawQuestionMark(img, size / 2, size / 2, 18, iconColor);
                    break;
                case MapNodeType.Shop:
                    DrawCoinMini(img, size / 2, size / 2, 14, iconColor);
                    break;
                case MapNodeType.RestSite:
                    DrawCampfireMini(img, size / 2, size / 2, 16, iconColor);
                    break;
                case MapNodeType.Treasure:
                    DrawChestMini(img, size / 2, size / 2, 16, iconColor);
                    break;
            }

            var tex = ImageTexture.CreateFromImage(img);
            _cache[cacheKey] = tex;
            return tex;
        }

        public ImageTexture GenerateEnergyOrb(int energyType, bool filled = true)
        {
            var cacheKey = $"orb_{energyType}_{filled}";
            if (_cache.TryGetValue(cacheKey, out var cached))
                return cached;

            const int size = 48;
            var img = Image.Create(size, size, false, Image.Format.Rgba8);
            img.Fill(new Color(0, 0, 0, 0));

            Color orbColor;
            switch (energyType)
            {
                case 0: orbColor = new Color(0.2f, 0.4f, 1f); break;
                case 1: orbColor = new Color(0.2f, 0.8f, 0.3f); break;
                case 2: orbColor = new Color(1f, 0.7f, 0.2f); break;
                default: orbColor = new Color(0.6f, 0.2f, 0.8f); break;
            }

            if (!filled)
                orbColor = orbColor.Darkened(0.6f);

            DrawCircleFilled(img, size / 2, size / 2, size / 2 - 4, orbColor.Darkened(0.4f));
            DrawCircleFilled(img, size / 2, size / 2, size / 2 - 8, orbColor);

            for (int r = size / 2 - 6; r <= size / 2 - 2; r++)
            {
                float alpha = 0.5f * (1f - (float)(r - (size / 2 - 6)) / 4f);
                var highlight = Colors.White;
                highlight.A = alpha;
                DrawArc(img, size / 2, size / 2, r, -Mathf.Pi * 0.7f, -Mathf.Pi * 0.2f, highlight, 2);
            }

            if (filled)
            {
                DrawCircleFilled(img, size / 2 - 6, size / 2 - 8, 6, orbColor.Lightened(0.6f));
                var spark = Colors.White;
                spark.A = 0.7f;
                DrawDot(img, size / 2 - 8, size / 2 - 10, 3, spark);
            }

            var tex = ImageTexture.CreateFromImage(img);
            _cache[cacheKey] = tex;
            return tex;
        }

        public ImageTexture GenerateRelicImage(string relicId, RelicTier tier)
        {
            var cacheKey = $"relic_{relicId}_{tier}";
            if (_cache.TryGetValue(cacheKey, out var cached))
                return cached;

            const int w = 80, h = 80;
            var img = Image.Create(w, h, false, Image.Format.Rgba8);
            img.Fill(new Color(0, 0, 0, 0));

            Color bgColor;
            switch (tier)
            {
                case RelicTier.Common: bgColor = new Color(0.4f, 0.5f, 0.4f); break;
                case RelicTier.Uncommon: bgColor = new Color(0.3f, 0.4f, 0.7f); break;
                case RelicTier.Rare: bgColor = new Color(0.6f, 0.3f, 0.6f); break;
                case RelicTier.Boss: bgColor = new Color(0.7f, 0.2f, 0.2f); break;
                case RelicTier.Special: bgColor = new Color(0.8f, 0.6f, 0.1f); break;
                default: bgColor = new Color(0.5f, 0.5f, 0.5f); break;
            }

            _rng.Seed = (ulong)(relicId.GetHashCode() & 0x7FFFFFFF);
            int shapeType = _rng.RandiRange(0, 5);

            DrawCircleFilled(img, w / 2, h / 2, w / 2 - 3, bgColor.Darkened(0.3f));
            DrawCircleFilled(img, w / 2, h / 2, w / 2 - 6, bgColor);

            switch (shapeType)
            {
                case 0: DrawDiamondShape(img, w / 2, h / 2, 22, Colors.White); break;
                case 1: DrawSmallStar(img, w / 2, h / 2, 20, Colors.White); break;
                case 2: DrawEyeShape(img, w / 2, h / 2, 18, Colors.White); break;
                case 3: DrawCrossShape(img, w / 2, h / 2, 20, Colors.White); break;
                case 4: DrawMoonShape(img, w / 2, h / 2, 20, Colors.White); break;
                default: DrawRingShape(img, w / 2, h / 2, 16, Colors.White); break;
            }

            var tex = ImageTexture.CreateFromImage(img);
            _cache[cacheKey] = tex;
            return tex;
        }

        public ImageTexture GeneratePotionImage(PotionType potionType)
        {
            var cacheKey = $"potion_{potionType}";
            if (_cache.TryGetValue(cacheKey, out var cached))
                return cached;

            const int size = 56;
            var img = Image.Create(size, size, false, Image.Format.Rgba8);
            img.Fill(new Color(0, 0, 0, 0));

            Color liquidColor;
            switch (potionType)
            {
                case PotionType.HealthPotion: liquidColor = new Color(0.9f, 0.2f, 0.3f); break;
                case PotionType.BlockPotion: liquidColor = new Color(0.2f, 0.4f, 0.9f); break;
                case PotionType.StrengthPotion: liquidColor = new Color(0.9f, 0.5f, 0.1f); break;
                case PotionType.DexterityPotion: liquidColor = new Color(0.2f, 0.8f, 0.4f); break;
                case PotionType.PoisonPotion: liquidColor = new Color(0.3f, 0.7f, 0.2f); break;
                case PotionType.EnergyPotion: liquidColor = new Color(0.3f, 0.3f, 0.9f); break;
                case PotionType.FirePotion: liquidColor = new Color(1f, 0.3f, 0f); break;
                case PotionType.FrozenPotion: liquidColor = new Color(0.5f, 0.8f, 1f); break;
                default: liquidColor = new Color(0.6f, 0.2f, 0.8f); break;
            }

            DrawBottleShape(img, size / 2, size / 2 + 4, liquidColor);

            var tex = ImageTexture.CreateFromImage(img);
            _cache[cacheKey] = tex;
            return tex;
        }

        public ImageTexture GenerateBackground(string bgType, Vector2I size)
        {
            var cacheKey = $"bg_{bgType}_{size.X}x{size.Y}";
            if (_cache.TryGetValue(cacheKey, out var cached))
                return cached;

            var img = Image.Create(size.X, size.Y, false, Image.Format.Rgba8);

            switch (bgType.ToLower())
            {
                case "mainmenu":
                    DrawMainMenuBG(img, size.X, size.Y);
                    break;
                case "combat":
                    DrawCombatBG(img, size.X, size.Y);
                    break;
                case "map":
                    DrawMapBG(img, size.X, size.Y);
                    break;
                case "characterselect":
                    DrawCharSelectBG(img, size.X, size.Y);
                    break;
                default:
                    img.Fill(new Color(0.08f, 0.06f, 0.12f, 1f));
                    break;
            }

            var tex = ImageTexture.CreateFromImage(img);
            _cache[cacheKey] = tex;
            return tex;
        }

        private void DrawMainMenuBG(Image img, int w, int h)
        {
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float nx = (float)x / w;
                    float ny = (float)y / h;
                    float v = Mathf.Sin(nx * 3f + ny * 2f) * 0.5f +
                              Mathf.Sin(nx * 5f - ny * 3f) * 0.3f +
                              Mathf.Sin(ny * 4f + Mathf.Sin(nx * 2f) * 2f) * 0.2f;
                    v = (v + 1f) * 0.5f;
                    float r = 0.04f + v * 0.06f;
                    float g = 0.02f + v * 0.03f;
                    float b = 0.08f + v * 0.08f;
                    img.SetPixel(x, y, new Color(r, g, b, 1f));
                }
            }

            for (int i = 0; i < 50; i++)
            {
                int sx = _rng.RandiRange(0, w - 1);
                int sy = _rng.RandiRange(0, h - 1);
                float brightness = _rng.RandfRange(0.3f, 0.8f);
                var starColor = new Color(brightness, brightness, brightness * 1.1f, _rng.RandfRange(0.3f, 0.7f));
                int ss = _rng.RandiRange(1, 3);
                DrawDot(img, sx, sy, ss, starColor);
            }
        }

        private void DrawCombatBG(Image img, int w, int h)
        {
            for (int y = 0; y < h; y++)
            {
                float t = (float)y / h;
                var topColor = new Color(0.15f, 0.08f, 0.12f);
                var bottomColor = new Color(0.08f, 0.05f, 0.1f);
                var col = topColor.Lerp(bottomColor, t);
                float noise = Mathf.Sin((float)y / 10f) * Mathf.Cos((float)y / 15f) * 0.02f;
                col += new Color(noise, noise, noise);
                for (int x = 0; x < w; x++)
                    img.SetPixel(x, y, col);
            }

            var floorColor = new Color(0.12f, 0.1f, 0.15f, 0.5f);
            int floorY = (int)(h * 0.72f);
            for (int y = floorY; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dist = (float)(y - floorY) / (h - floorY);
                    var c = floorColor;
                    c.A = 0.3f + dist * 0.4f;
                    var existing = img.GetPixel(x, y);
                    img.SetPixel(x, y, existing.Lerp(c, c.A));
                }
            }
        }

        private void DrawMapBG(Image img, int w, int h)
        {
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float nx = (float)x / w * 4f;
                    float ny = (float)y / h * 4f;
                    float v = (Mathf.Sin(nx * 10f) + Mathf.Sin(ny * 10f)) * 0.25f + 0.5f;
                    float r = 0.03f + v * 0.04f;
                    float g = 0.05f + v * 0.05f;
                    float b = 0.08f + v * 0.06f;
                    img.SetPixel(x, y, new Color(r, g, b, 1f));
                }
            }

            var gridColor = new Color(1f, 1f, 1f, 0.03f);
            int gridSize = 40;
            for (int x = 0; x < w; x += gridSize)
                DrawLineVertical(img, x, 0, h, gridColor);
            for (int y = 0; y < h; y += gridSize)
                DrawLineHorizontal(img, 0, w, y, gridColor);
        }

        private void DrawCharSelectBG(Image img, int w, int h)
        {
            for (int y = 0; y < h; y++)
            {
                float t = (float)y / h;
                var topCol = new Color(0.06f, 0.04f, 0.1f);
                var botCol = new Color(0.12f, 0.06f, 0.14f);
                var col = topCol.Lerp(botCol, t);
                for (int x = 0; x < w; x++)
                {
                    float pulse = Mathf.Sin((float)x / 80f + (float)y / 60f) * 0.015f;
                    img.SetPixel(x, y, col + new Color(pulse, pulse, pulse * 0.5f));
                }
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
                                BlendPixel(img, rx, ry, color);
                        }
                        else
                        {
                            BlendPixel(img, rx, ry, color);
                        }
                    }
                }
            }
        }

        private void DrawRectFilled(Image img, int x, int y, int w, int h, Color color)
        {
            for (int dy = 0; dy < h; dy++)
                for (int dx = 0; dx < w; dx++)
                    SetPixelSafe(img, x + dx, y + dy, color);
        }

        private void DrawCircleFilled(Image img, int cx, int cy, int radius, Color color)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    if (dx * dx + dy * dy <= radius * radius)
                        SetPixelSafe(img, cx + dx, cy + dy, color);
                }
            }
        }

        private void DrawCircleOutline(Image img, int cx, int cy, int radius, Color color, int thickness = 1)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (Mathf.Abs(dist - radius) < thickness)
                        SetPixelSafe(img, cx + dx, cy + dy, color);
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
                        SetPixelSafe(img, cx + dx, cy + dy, color);
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

        private void DrawLineVertical(Image img, int x, int y0, int y1, Color color)
        {
            for (int y = y0; y < y1; y++)
                SetPixelSafe(img, x, y, color);
        }

        private void DrawLineHorizontal(Image img, int x0, int x1, int y, Color color)
        {
            for (int x = x0; x < x1; x++)
                SetPixelSafe(img, x, y, color);
        }

        private void DrawDot(Image img, int x, int y, int size, Color color)
        {
            for (int dy = -size; dy <= size; dy++)
                for (int dx = -size; dx <= size; dx++)
                    if (dx * dx + dy * dy <= size * size)
                        SetPixelSafe(img, x + dx, y + dy, color);
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

        private void SetPixelSafe(Image img, int x, int y, Color color)
        {
            if (x >= 0 && x < img.GetWidth() && y >= 0 && y < img.GetHeight())
                img.SetPixel(x, y, color);
        }

        private void BlendPixel(Image img, int x, int y, Color color)
        {
            if (x >= 0 && x < img.GetWidth() && y >= 0 && y < img.GetHeight())
            {
                var existing = img.GetPixel(x, y);
                img.SetPixel(x, y, existing.Lerp(color, color.A));
            }
        }

        #endregion

        #region Symbol Drawing

        private void DrawSwordSymbol(Image img, int cx, int cy, int size, Color color)
        {
            int half = size / 2;
            DrawLineThick(img, cx - half + 10, cy + half, cx + 5, cy - half + 10, color, 4);
            DrawLineThick(img, cx + 5, cy - half + 10, cx + half - 5, cy - half + 20, color, 3);
            DrawLineThick(img, cx - 8, cy + half - 5, cx + 8, cy + half + 5, color, 4);
            DrawLineThick(img, cx - half + 5, cy + half + 5, cx - half + 15, cy + half + 15, color, 3);
            DrawLineThick(img, cx + half - 15, cy + half + 5, cx + half - 5, cy + half + 15, color, 3);
        }

        private void DrawShieldSymbol(Image img, int cx, int cy, int size, Color color)
        {
            int half = size / 2;
            DrawLineThick(img, cx - half + 5, cy - half + 10, cx + half - 5, cy - half + 10, color, 4);
            DrawLineThick(img, cx - half + 5, cy - half + 10, cx - half + 10, cy - half + 25, color, 3);
            DrawLineThick(img, cx + half - 5, cy - half + 10, cx + half - 10, cy - half + 25, color, 3);
            DrawLineThick(img, cx - half + 10, cy - half + 25, cx, cy + half - 5, color, 4);
            DrawLineThick(img, cx + half - 10, cy - half + 25, cx, cy + half - 5, color, 4);
            DrawLineThick(img, cx - 12, cy + half - 5, cx + 12, cy + half - 5, color, 4);
        }

        private void DrawStarSymbol(Image img, int cx, int cy, int size, Color color)
        {
            DrawSmallStar(img, cx, cy, size / 2, color);
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

        private void DrawArrowSymbol(Image img, int cx, int cy, int size, Color color)
        {
            int half = size / 2;
            DrawLineThick(img, cx, cy - half + 5, cx, cy + half - 10, color, 3);
            DrawLineThick(img, cx, cy - half + 5, cx - half / 2, cy - half + 20, color, 3);
            DrawLineThick(img, cx, cy - half + 5, cx + half / 2, cy - half + 20, color, 3);
        }

        private void DrawSkullSymbol(Image img, int cx, int cy, int size, Color color)
        {
            DrawEllipseFilled(img, cx, cy - 5, size / 2, size / 2 - 3, color);
            DrawRectFilled(img, cx - size / 3, cy + size / 4, size * 2 / 3, size / 4, color);
            DrawCircleFilled(img, cx - size / 4, cy - size / 5, size / 7, new Color(0.05f, 0.05f, 0.08f));
            DrawCircleFilled(img, cx + size / 4, cy - size / 5, size / 7, new Color(0.05f, 0.05f, 0.08f));
            DrawLineThick(img, cx - size / 5, cy + size / 6, cx - size / 8, cy + size / 4 - 2, new Color(0.05f, 0.05f, 0.08f), 2);
            DrawLineThick(img, cx + size / 8, cy + size / 6, cx + size / 5, cy + size / 4 - 2, new Color(0.05f, 0.05f, 0.08f), 2);
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
                        SetPixelSafe(img, x, y, color);
                }
            }
        }

        private void DrawAntenna(Image img, int cx, int topY, Color color)
        {
            DrawLineThick(img, cx, topY, cx, topY - 15, color, 2);
            DrawCircleFilled(img, cx, topY - 17, 4, color.Lightened(0.3f));
        }

        private void DrawOrbFloat(Image img, int cx, int cy, int radius, Color color)
        {
            DrawCircleFilled(img, cx, cy, radius, color.Darkened(0.3f));
            DrawCircleFilled(img, cx, cy, radius - 2, color);
            DrawCircleFilled(img, cx - radius / 3, cy - radius / 3, radius / 3, color.Lightened(0.4f));
        }

        private void DrawSmallIcon(Image img, int x, int y, string className, Color themeColor)
        {
            int size = 12;
            var c = themeColor.Lightened(0.4f);
            c.A = 0.5f;
            switch (className.ToLower())
            {
                case "ironclad": DrawRectFilled(img, x - size / 2, y - size / 2, size, size, c); break;
                case "silent": DrawTriangleFilled(img, x, y - size / 2, x - size / 2, y + size / 2, x + size / 2, y + size / 2, c); break;
                case "defect": DrawCircleFilled(img, x, y, size / 2, c); break;
                case "watcher": DrawSmallStar(img, x, y, size / 2, c); break;
                default: DrawCircleFilled(img, x, y, size / 2, c); break;
            }
        }

        #endregion

        #region Icon Drawing

        private void DrawSwordIcon(Image img, int size, Color color)
        {
            int c = size / 2;
            DrawLineThick(img, c - 8, c + 14, c + 3, c - 12, color, 3);
            DrawLineThick(img, c + 3, c - 12, c + 10, c - 6, color, 3);
            DrawLineThick(img, c - 6, c + 14, c + 6, c + 14, color, 4);
            DrawLineThick(img, c - 8, c + 18, c - 4, c + 22, color, 2);
            DrawLineThick(img, c + 4, c + 18, c + 8, c + 22, color, 2);
        }

        private void DrawShieldIcon(Image img, int size, Color color)
        {
            int c = size / 2;
            DrawLineThick(img, c - 12, c - 14, c + 12, c - 14, color, 3);
            DrawLineThick(img, c - 12, c - 14, c - 14, c - 6, color, 2);
            DrawLineThick(img, c + 12, c - 14, c + 14, c - 6, color, 2);
            DrawLineThick(img, c - 14, c - 6, c, c + 16, color, 3);
            DrawLineThick(img, c + 14, c - 6, c, c + 16, color, 3);
        }

        private void DrawEnergyIcon(Image img, int size, Color color)
        {
            int c = size / 2;
            DrawCircleFilled(img, c, c, size / 2 - 4, color.Darkened(0.3f));
            DrawCircleFilled(img, c, c, size / 2 - 7, color);
            DrawLightning(img, c, c, 10, Colors.White);
        }

        private void DrawLightning(Image img, int cx, int cy, int size, Color color)
        {
            int[] ptsX = { cx, cx + size / 3, cx, cx - size / 3, cx };
            int[] ptsY = { cy - size, cy - size / 4, cy + size / 6, cy + size / 2, cy + size };
            for (int i = 0; i < 4; i++)
                DrawLineThick(img, ptsX[i], ptsY[i], ptsX[i + 1], ptsY[i + 1], color, 2);
        }

        private void DrawHeartIcon(Image img, int size, Color color)
        {
            int c = size / 2;
            DrawCircleFilled(img, c - 6, c - 3, 7, color);
            DrawCircleFilled(img, c + 6, c - 3, 7, color);
            DrawTriangleFilled(img, c - 12, c + 1, c + 12, c + 1, c, c + 16, color);
        }

        private void DrawGoldIcon(Image img, int size, Color color)
        {
            int c = size / 2;
            DrawCircleFilled(img, c, c + 2, size / 2 - 4, color.Darkened(0.2f));
            DrawCircleFilled(img, c, c, size / 2 - 6, color);
            DrawCircleFilled(img, c - 4, c - 3, 4, color.Lightened(0.3f));
            char[] sym = { '$', 'G', '¥' };
            DrawTextPixel(img, c - 3, c + 4, sym[_rng.RandiRange(0, 2)], Colors.White.Darkened(0.3f));
        }

        private void DrawTextPixel(Image img, int x, int y, char ch, Color color)
        {
        }

        private void DrawPotionIcon(Image img, int size, Color color)
        {
            DrawBottleShape(img, size / 2, size / 2 + 4, color);
        }

        private void DrawBottleShape(Image img, int cx, int cy, Color liquidColor)
        {
            DrawRectFilled(img, cx - 4, cy - 16, 8, 6, new Color(0.7f, 0.7f, 0.75f));
            DrawEllipseFilled(img, cx, cy, 10, 14, new Color(0.7f, 0.7f, 0.75f));
            DrawEllipseFilled(img, cx, cy + 3, 7, 10, liquidColor);
            DrawCircleFilled(img, cx, cy - 10, 4, new Color(0.8f, 0.85f, 0.9f));
            DrawRectFilled(img, cx - 1, cy - 19, 2, 4, new Color(0.7f, 0.7f, 0.75f));
        }

        private void DrawRelicIcon(Image img, int size, Color color)
        {
            int c = size / 2;
            DrawDiamondShape(img, c, c, size / 3, color);
        }

        private void DrawDiamondShape(Image img, int cx, int cy, int size, Color color)
        {
            DrawTriangleFilled(img, cx, cy - size, cx - size, cy, cx, cy + size, color);
            DrawTriangleFilled(img, cx, cy - size, cx + size, cy, cx, cy + size, color);
        }

        private void DrawMapNodeIcon(Image img, int size, Color color)
        {
            DrawCircleFilled(img, size / 2, size / 2, size / 2 - 4, color);
        }

        private void DrawGearIcon(Image img, int size, Color color)
        {
            int c = size / 2;
            int outer = size / 2 - 6;
            int inner = size / 4;
            DrawCircleFilled(img, c, c, outer, color);
            DrawCircleFilled(img, c, c, inner, new Color(0.1f, 0.1f, 0.12f));
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.Pi / 4f;
                int x1 = c + (int)(Mathf.Cos(a) * inner);
                int y1 = c + (int)(Mathf.Sin(a) * inner);
                int x2 = c + (int)(Mathf.Cos(a) * outer);
                int y2 = c + (int)(Mathf.Sin(a) * outer);
                DrawLineThick(img, x1, y1, x2, y2, color, 3);
            }
        }

        private void DrawXIcon(Image img, int size, Color color)
        {
            int m = 6;
            DrawLineThick(img, m, m, size - m, size - m, color, 3);
            DrawLineThick(img, size - m, m, m, size - m, color, 3);
        }

        private void DrawArrowIcon(Image img, int size, Color color)
        {
            int c = size / 2;
            DrawLineThick(img, size / 4, c, size * 3 / 4, c, color, 3);
            DrawLineThick(img, size * 3 / 4, c, size / 2 + 2, c - 6, color, 2);
            DrawLineThick(img, size * 3 / 4, c, size / 2 + 2, c + 6, color, 2);
        }

        private void DrawArrowLeftIcon(Image img, int size, Color color)
        {
            int c = size / 2;
            DrawLineThick(img, size * 3 / 4, c, size / 4, c, color, 3);
            DrawLineThick(img, size / 4, c, size / 2 - 2, c - 6, color, 2);
            DrawLineThick(img, size / 4, c, size / 2 - 2, c + 6, color, 2);
        }

        private void DrawSkullMini(Image img, int cx, int cy, int size, Color color)
        {
            DrawEllipseFilled(img, cx, cy - 2, size, size - 3, color);
            DrawRectFilled(img, cx - size * 2 / 5, cy + size / 3 - 2, size * 4 / 5, size / 3, color);
            DrawCircleFilled(img, cx - size / 3, cy - size / 5, size / 6, new Color(0.1f, 0.08f, 0.12f));
            DrawCircleFilled(img, cx + size / 3, cy - size / 5, size / 6, new Color(0.1f, 0.08f, 0.12f));
        }

        private void DrawQuestionMark(Image img, int cx, int cy, int size, Color color)
        {
            DrawLineThick(img, cx - 2, cy - size + 4, cx + 3, cy - size / 3, color, 3);
            DrawLineThick(img, cx + 3, cy - size / 3, cx + 5, cy - size / 3, color, 3);
            DrawDot(img, cx, cy + size / 4, 3, color);
        }

        private void DrawCoinMini(Image img, int cx, int cy, int radius, Color color)
        {
            DrawCircleFilled(img, cx, cy, radius, color.Darkened(0.2f));
            DrawCircleFilled(img, cx, cy - 2, radius - 3, color);
            DrawCircleFilled(img, cx - radius / 3, cy - radius / 3, radius / 4, color.Lightened(0.4f));
        }

        private void DrawCampfireMini(Image img, int cx, int cy, int size, Color color)
        {
            DrawLineThick(img, cx - size, cy + size / 2, cx + size, cy + size / 2, new Color(0.4f, 0.25f, 0.15f), 3);
            DrawLineThick(img, cx - size + 2, cy + size / 2 + 3, cx + size - 2, cy + size / 2 + 3, new Color(0.3f, 0.18f, 0.1f), 2);
            DrawTriangleFilled(img, cx, cy - size, cx - size / 2, cy + size / 3, cx + size / 2, cy + size / 3, new Color(1f, 0.5f, 0.1f));
            DrawTriangleFilled(img, cx, cy - size + 4, cx - size / 3, cy + size / 4, cx + size / 3, cy + size / 4, new Color(1f, 0.7f, 0.2f));
        }

        private void DrawChestMini(Image img, int cx, int cy, int size, Color color)
        {
            DrawRectFilled(img, cx - size, cy - size / 3, size * 2, size * 2 / 3, color.Darkened(0.2f));
            DrawRectFilled(img, cx - size + 2, cy - size / 3 + 2, size * 2 - 4, size * 2 / 3 - 4, color);
            DrawRectFilled(img, cx - 3, cy - size / 3 - 2, 6, size * 2 / 3 + 4, color.Lightened(0.2f));
            DrawCircleFilled(img, cx, cy, size / 4, color.Lightened(0.4f));
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

        #endregion

        public void ClearCache()
        {
            foreach (var kv in _cache)
                kv.Value.Dispose();
            _cache.Clear();
        }

        public int CacheCount => _cache.Count;
    }

    public enum IconType
    {
        Attack,
        Defend,
        Energy,
        Health,
        Gold,
        Potion,
        Relic,
        MapNode,
        Settings,
        Close,
        ArrowRight,
        ArrowLeft
    }

    public enum MapNodeType
    {
        Monster,
        Elite,
        Boss,
        Event,
        Shop,
        RestSite,
        Treasure,
        Unknown
    }

    public enum RelicTier
    {
        Common,
        Uncommon,
        Rare,
        Boss,
        Special
    }

    public enum PotionType
    {
        HealthPotion,
        BlockPotion,
        StrengthPotion,
        DexterityPotion,
        PoisonPotion,
        EnergyPotion,
        FirePotion,
        FrozenPotion,
        RandomPotion
    }
}
