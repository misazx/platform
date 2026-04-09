using Godot;
using RoguelikeGame.Core;
using System;
using System.Collections.Generic;
using RoguelikeGame.Database;

namespace RoguelikeGame.Systems
{
    public enum RestOption
    {
        Heal,
        Upgrade,
        Smith,
        Recall
    }

    public partial class RestSiteManager : Node
    {
        public static RestSiteManager Instance { get; private set; }

        [Signal]
        public delegate void RestCompletedEventHandler(RestOption option);

        public override void _Ready()
        {
            if (Instance != null && Instance != this)
            {
                QueueFree();
                return;
            }
            Instance = this;
        }

        public List<RestOption> GetAvailableOptions(Node player)
        {
            var options = new List<RestOption>
            {
                RestOption.Heal,
                RestOption.Upgrade
            };

            // Check for special conditions
            if (HasRecallOption(player))
            {
                options.Add(RestOption.Recall);
            }

            return options;
        }

        public bool HasRecallOption(Node player)
        {
            // Check if player has specific relic that allows recall
            if (player.HasMethod("HasRelic") && (bool)player.Call("HasRelic", "dream_catcher"))
            {
                return true;
            }
            return false;
        }

        public void PerformRestAction(RestOption option, Node player)
        {
            switch (option)
            {
                case RestOption.Heal:
                    HealPlayer(player);
                    break;
                    
                case RestOption.Upgrade:
                    OpenUpgradeUI(player);
                    break;
                    
                case RestOption.Recall:
                    RecallCards(player);
                    break;
            }

            EmitSignal(SignalName.RestCompleted, option.ToString());

            TimelineManager.Instance?.AddEventTriggered(
                "rest_site",
                $"选择了: {option}",
                GameManager.Instance?.CurrentRun?.CurrentFloor ?? 1,
                GameManager.Instance?.CurrentRun?.CurrentRoom ?? 0
            );

            GD.Print($"[RestSiteManager] Performed rest action: {option}");
        }

        private void HealPlayer(Node player)
        {
            int healAmount = 30; // Standard heal amount
            
            if (player.HasMethod("Heal"))
            {
                player.Call("Heal", healAmount);
                GD.Print($"[RestSiteManager] Healed {healAmount} HP");
            }
        }

        private void OpenUpgradeUI(Node player)
        {
            // In a real implementation, this would open a UI to select cards to upgrade
            // For now, we'll just log it
            GD.Print("[RestSiteManager] Opening upgrade UI...");
            
            // Could also auto-upgrade a random card
            if (player.HasMethod("UpgradeRandomCard"))
            {
                player.Call("UpgradeRandomCard");
            }
        }

        private void RecallCards(Node player)
        {
            // Return all discarded cards to hand
            if (player.HasMethod("RecallDiscardedCards"))
            {
                player.Call("RecallDiscardedCards");
                GD.Print("[RestSiteManager] Recalled discarded cards");
            }
        }

        public string GetRestOptionDescription(RestOption option)
        {
            return option switch
            {
                RestOption.Heal => "回复 30% 最大生命值",
                RestOption.Upgrade => "升级一张牌",
                RestOption.Recall => "将弃牌堆中的所有牌收回手牌",
                RestOption.Smith => "移除一张牌（需要特定遗物）",
                _ => "未知选项"
            };
        }

        public string GetRestOptionIcon(RestOption option)
        {
            return option switch
            {
                RestOption.Heal => "res://GameModes/base_game/Resources/Icons/Rest/heal.png",
                RestOption.Upgrade => "res://GameModes/base_game/Resources/Icons/Rest/upgrade.png",
                RestOption.Recall => "res://GameModes/base_game/Resources/Icons/Rest/recall.png",
                RestOption.Smith => "res://GameModes/base_game/Resources/Icons/Rest/smith.png",
                _ => "res://GameModes/base_game/Resources/Icons/Rest/default.png"
            };
        }
    }
}
