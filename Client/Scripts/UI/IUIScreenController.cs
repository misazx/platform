using Godot;

namespace RoguelikeGame.UI
{
    public interface IUIScreenController
    {
        void OnScreenReady(Control screen);
        void OnScreenHide();
    }
}
