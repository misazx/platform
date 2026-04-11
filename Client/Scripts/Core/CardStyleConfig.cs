using Godot;
using System.Collections.Generic;
using RoguelikeGame.Database;

namespace RoguelikeGame.Core
{
    public class CardStyleConfig
    {
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

        public static readonly Dictionary<CardType, Color> TypeColors = new()
        {
            { CardType.Attack, AttackColor },
            { CardType.Skill, SkillColor },
            { CardType.Power, PowerColor },
            { CardType.Status, StatusColor },
            { CardType.Curse, CurseColor }
        };

        public static readonly Dictionary<CardRarity, Color> RarityBorders = new()
        {
            { CardRarity.Basic, BasicBorder },
            { CardRarity.Common, CommonBorder },
            { CardRarity.Uncommon, UncommonBorder },
            { CardRarity.Rare, RareBorder },
            { CardRarity.Special, SpecialBorder }
        };

        public static Color GetTypeColor(CardType type)
        {
            return TypeColors.TryGetValue(type, out var color) ? color : new Color(0.3f, 0.3f, 0.3f);
        }

        public static Color GetRarityBorder(CardRarity rarity)
        {
            return RarityBorders.TryGetValue(rarity, out var color) ? color : BasicBorder;
        }
    }
}
