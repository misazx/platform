using Godot;

namespace RoguelikeGame.UI
{
    public interface ICombatUI
    {
        void OnCombatStart();
        void OnCombatEnd(bool victory);
        void OnTurnChanged(bool isPlayerTurn);
    }

    public interface IAchievementUI
    {
        void ShowAchievement(string achievementId);
        void UpdateProgress(string achievementId, float progress);
    }

    public interface IMapUI
    {
        void OnMapGenerated(string mapJson);
        void OnNodeSelected(int nodeId);
        void OnFloorChanged(int floor);
    }

    public interface IShopUI
    {
        void OnShopEntered();
        void OnItemPurchased(string itemId);
        void OnShopClosed();
    }

    public interface IRestSiteUI
    {
        void OnRestSiteEntered();
        void OnRestCompleted();
    }
}
