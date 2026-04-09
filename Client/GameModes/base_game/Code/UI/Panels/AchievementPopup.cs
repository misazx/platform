using Godot;
using System;

namespace RoguelikeGame.UI.Panels
{
    public partial class AchievementPopup : Control
    {
        private Label _titleLabel;
        private Label _descLabel;
        private Tween _tween;

        public override void _Ready()
        {
            _titleLabel = GetNode<Label>("Panel/HBox/VBox/TitleLabel");
            _descLabel = GetNode<Label>("Panel/HBox/VBox/DescLabel");

            Hide();
        }

        public void ShowAchievement(string title, string description)
        {
            if (_titleLabel != null)
                _titleLabel.Text = title;

            if (_descLabel != null)
                _descLabel.Text = description;

            Show();
            PlayAnimation();
        }

        private void PlayAnimation()
        {
            _tween?.Kill();
            _tween = CreateTween();

            Modulate = new Color(1, 1, 1, 0);
            Position = new Vector2(Position.X, -100);

            _tween.TweenProperty(this, "modulate:a", 1f, 0.3f);
            _tween.Parallel().TweenProperty(this, "position:y", 50f, 0.3f);
            _tween.TweenInterval(3f);
            _tween.TweenProperty(this, "modulate:a", 0f, 0.5f);
            _tween.TweenCallback(Callable.From(Hide));
        }
    }
}
