using BTTree = RoguelikeGame.Shared.BehaviorTree.BehaviorTree;
using RoguelikeGame.Shared.BehaviorTree;

namespace RoguelikeGame.Shared.Bots
{
    public enum CardType { ATTACK, SKILL, POWER }
    public enum CardTargetType { ENEMY_SINGLE, ENEMY_ALL, SELF, ALL, NONE }
    public enum EnemyIntentType { ATTACK, DEFEND, BUFF, DEBUFF, UNKNOWN }
    public enum PotionType { HEALING, DAMAGE, SHIELD, UTILITY }

    public class CardData
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public int Cost { get; set; }
        public CardType Type { get; set; }
        public int Damage { get; set; }
        public int Block { get; set; }
        public CardTargetType TargetType { get; set; }
    }

    public class EnemyData
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public int Block { get; set; }
        public EnemyIntentType IntentType { get; set; }
        public int IntentDamage { get; set; }
    }

    public class PotionData
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public PotionType Type { get; set; }
        public int Value { get; set; }
    }

    public static class BotBBKeys
    {
        public const string Hand = "hand";
        public const string PlayerHp = "player_hp";
        public const string PlayerMaxHp = "player_max_hp";
        public const string PlayerEnergy = "player_energy";
        public const string PlayerBlock = "player_block";
        public const string Enemies = "enemies";
        public const string Potions = "potions";
        public const string SelectedCard = "selected_card";
        public const string SelectedTarget = "selected_target";
        public const string SelectedPotion = "selected_potion";
        public const string ActionType = "action_type";
    }

    public static class BotActions
    {
        public const string PlayCard = "play_card";
        public const string UsePotion = "use_potion";
        public const string EndTurn = "end_turn";
    }

    public class CardBotAI
    {
        private readonly BotDifficulty _difficulty;
        private const double LowHpThreshold = 0.3;
        private const double CriticalHpThreshold = 0.5;

        public CardBotAI(BotDifficulty difficulty) { _difficulty = difficulty; }

        public BTCondition IsLowHP() => new(context =>
        {
            int hp = context.Blackboard.Get<int>(BotBBKeys.PlayerHp);
            int maxHp = context.Blackboard.Get<int>(BotBBKeys.PlayerMaxHp);
            return maxHp > 0 && hp < maxHp * LowHpThreshold;
        }, "IsLowHP");

        public BTCondition HasAttackCard() => new(context =>
        {
            var hand = context.Blackboard.Get<List<CardData>>(BotBBKeys.Hand);
            return hand != null && hand.Any(c => c.Type == CardType.ATTACK);
        }, "HasAttackCard");

        public BTCondition HasDefenseCard() => new(context =>
        {
            var hand = context.Blackboard.Get<List<CardData>>(BotBBKeys.Hand);
            return hand != null && hand.Any(c => c.Block > 0 || (c.Type == CardType.SKILL && c.TargetType == CardTargetType.SELF));
        }, "HasDefenseCard");

        public BTCondition HasPowerCard() => new(context =>
        {
            var hand = context.Blackboard.Get<List<CardData>>(BotBBKeys.Hand);
            return hand != null && hand.Any(c => c.Type == CardType.POWER);
        }, "HasPowerCard");

        public BTCondition HasHealingPotion() => new(context =>
        {
            int hp = context.Blackboard.Get<int>(BotBBKeys.PlayerHp);
            int maxHp = context.Blackboard.Get<int>(BotBBKeys.PlayerMaxHp);
            if (maxHp <= 0 || hp >= maxHp * CriticalHpThreshold) return false;
            var potions = context.Blackboard.Get<List<PotionData>>(BotBBKeys.Potions);
            return potions != null && potions.Any(p => p.Type == PotionType.HEALING);
        }, "HasHealingPotion");

        public BTAction PlayBestAttack() => new(context =>
        {
            var hand = context.Blackboard.Get<List<CardData>>(BotBBKeys.Hand);
            int energy = context.Blackboard.Get<int>(BotBBKeys.PlayerEnergy);
            var enemies = context.Blackboard.Get<List<EnemyData>>(BotBBKeys.Enemies);
            if (hand == null || enemies == null || enemies.Count == 0) return BTNodeStatus.Failure;

            var attacks = hand.Where(c => c.Type == CardType.ATTACK && c.Cost <= energy).ToList();
            if (attacks.Count == 0) return BTNodeStatus.Failure;

            CardData? selected = null;
            string? targetId = null;

            if (_difficulty == BotDifficulty.Easy)
            {
                selected = attacks[Random.Shared.Next(attacks.Count)];
            }
            else if (_difficulty == BotDifficulty.Hard)
            {
                var singleAttacks = attacks.Where(c => c.TargetType == CardTargetType.ENEMY_SINGLE).ToList();
                var aoeAttacks = attacks.Where(c => c.TargetType == CardTargetType.ENEMY_ALL).ToList();

                foreach (var card in singleAttacks.OrderByDescending(c => c.Damage))
                {
                    var killable = enemies.FirstOrDefault(e => e.Hp > 0 && e.Hp - e.Block <= card.Damage);
                    if (killable != null) { selected = card; targetId = killable.Id; break; }
                }

                if (selected == null && aoeAttacks.Count > 0 && enemies.Count(e => e.Hp > 0) >= 2)
                    selected = aoeAttacks.OrderByDescending(c => c.Damage).First();

                selected ??= attacks.OrderByDescending(c => (double)c.Damage / Math.Max(c.Cost, 1)).First();
            }
            else
            {
                selected = attacks.OrderByDescending(c => (double)c.Damage / Math.Max(c.Cost, 1)).First();
            }

            if (targetId == null && selected.TargetType == CardTargetType.ENEMY_SINGLE)
            {
                var target = enemies.Where(e => e.Hp > 0).OrderBy(e => e.Hp).FirstOrDefault();
                if (target != null) targetId = target.Id;
            }

            context.Blackboard.Set(BotBBKeys.SelectedCard, selected);
            context.Blackboard.Set(BotBBKeys.ActionType, BotActions.PlayCard);
            if (targetId != null) context.Blackboard.Set(BotBBKeys.SelectedTarget, targetId);
            return BTNodeStatus.Success;
        }, "PlayBestAttack");

        public BTAction PlayDefense() => new(context =>
        {
            var hand = context.Blackboard.Get<List<CardData>>(BotBBKeys.Hand);
            int energy = context.Blackboard.Get<int>(BotBBKeys.PlayerEnergy);
            var enemies = context.Blackboard.Get<List<EnemyData>>(BotBBKeys.Enemies);
            int hp = context.Blackboard.Get<int>(BotBBKeys.PlayerHp);
            int maxHp = context.Blackboard.Get<int>(BotBBKeys.PlayerMaxHp);
            int block = context.Blackboard.Get<int>(BotBBKeys.PlayerBlock);
            if (hand == null) return BTNodeStatus.Failure;

            var defenseCards = hand.Where(c => c.Cost <= energy && (c.Block > 0 || (c.Type == CardType.SKILL && c.TargetType == CardTargetType.SELF))).ToList();
            if (defenseCards.Count == 0) return BTNodeStatus.Failure;

            bool enemyAttacking = enemies != null && enemies.Any(e => e.IntentType == EnemyIntentType.ATTACK && e.Hp > 0);
            bool isLowHP = maxHp > 0 && hp < maxHp * CriticalHpThreshold;
            if (!enemyAttacking && !isLowHP) return BTNodeStatus.Failure;

            CardData selected;
            if (_difficulty == BotDifficulty.Easy)
            {
                selected = defenseCards[Random.Shared.Next(defenseCards.Count)];
            }
            else if (_difficulty == BotDifficulty.Hard)
            {
                int totalDmg = enemies?.Where(e => e.IntentType == EnemyIntentType.ATTACK && e.Hp > 0).Sum(e => e.IntentDamage) ?? 0;
                int effectiveDmg = Math.Max(0, totalDmg - block);
                selected = defenseCards.OrderByDescending(c => (double)c.Block / Math.Max(c.Cost, 1))
                    .FirstOrDefault(c => c.Block >= effectiveDmg)
                    ?? defenseCards.OrderByDescending(c => (double)c.Block / Math.Max(c.Cost, 1)).First();
            }
            else
            {
                selected = defenseCards.OrderByDescending(c => (double)c.Block / Math.Max(c.Cost, 1)).First();
            }

            context.Blackboard.Set(BotBBKeys.SelectedCard, selected);
            context.Blackboard.Set(BotBBKeys.ActionType, BotActions.PlayCard);
            return BTNodeStatus.Success;
        }, "PlayDefense");

        public BTAction PlayPower() => new(context =>
        {
            var hand = context.Blackboard.Get<List<CardData>>(BotBBKeys.Hand);
            int energy = context.Blackboard.Get<int>(BotBBKeys.PlayerEnergy);
            if (hand == null) return BTNodeStatus.Failure;

            var powerCards = hand.Where(c => c.Type == CardType.POWER && c.Cost <= energy).ToList();
            if (powerCards.Count == 0) return BTNodeStatus.Failure;

            CardData selected = _difficulty == BotDifficulty.Easy
                ? powerCards[Random.Shared.Next(powerCards.Count)]
                : _difficulty == BotDifficulty.Hard
                    ? powerCards.OrderBy(c => c.Cost).First()
                    : powerCards.First();

            context.Blackboard.Set(BotBBKeys.SelectedCard, selected);
            context.Blackboard.Set(BotBBKeys.ActionType, BotActions.PlayCard);
            return BTNodeStatus.Success;
        }, "PlayPower");

        public BTAction UseHealingPotion() => new(context =>
        {
            var potions = context.Blackboard.Get<List<PotionData>>(BotBBKeys.Potions);
            if (potions == null) return BTNodeStatus.Failure;
            var healing = potions.FirstOrDefault(p => p.Type == PotionType.HEALING);
            if (healing == null) return BTNodeStatus.Failure;
            context.Blackboard.Set(BotBBKeys.SelectedPotion, healing);
            context.Blackboard.Set(BotBBKeys.ActionType, BotActions.UsePotion);
            return BTNodeStatus.Success;
        }, "UseHealingPotion");

        public BTAction EndTurn() => new(context =>
        {
            context.Blackboard.Set(BotBBKeys.ActionType, BotActions.EndTurn);
            return BTNodeStatus.Success;
        }, "EndTurn");
    }

    public static class CardBotAIFactory
    {
        public static BTTree CreateBehaviorTree(BotDifficulty difficulty)
        {
            var ai = new CardBotAI(difficulty);
            var tree = new BTTree("CardBotAI_" + difficulty);

            var emergencyDefense = new BTSequence("EmergencyDefense")
                .AddChild(ai.IsLowHP())
                .AddChild(new BTSelector("EmergencyOptions")
                    .AddChild(new BTSequence("Heal")
                        .AddChild(ai.HasHealingPotion())
                        .AddChild(ai.UseHealingPotion()))
                    .AddChild(new BTSequence("Defend")
                        .AddChild(ai.HasDefenseCard())
                        .AddChild(ai.PlayDefense())));

            var powerSetup = new BTSequence("PowerSetup")
                .AddChild(ai.HasPowerCard())
                .AddChild(ai.PlayPower());

            var attack = new BTSequence("Attack")
                .AddChild(ai.HasAttackCard())
                .AddChild(ai.PlayBestAttack());

            var defense = new BTSequence("Defense")
                .AddChild(ai.HasDefenseCard())
                .AddChild(ai.PlayDefense());

            var root = new BTSelector("Root")
                .AddChild(emergencyDefense)
                .AddChild(powerSetup)
                .AddChild(attack)
                .AddChild(defense)
                .AddChild(ai.EndTurn());

            tree.Root = root;
            return tree;
        }
    }
}
