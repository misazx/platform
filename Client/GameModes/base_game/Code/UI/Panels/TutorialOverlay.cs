using Godot;
using System.Collections.Generic;

namespace RoguelikeGame.UI.Panels
{
    public partial class TutorialOverlay : Control
    {
        private RichTextLabel _contentLabel;
        private Button _nextButton;
        private List<string> _pages = new();
        private int _currentPage = 0;

        public override void _Ready()
        {
            _contentLabel = GetNode<RichTextLabel>("Panel/VBox/ContentLabel");
            _nextButton = GetNode<Button>("Panel/VBox/NextButton");
            var skipBtn = GetNode<Button>("Panel/VBox/SkipButton");

            _nextButton.Pressed += OnNextPressed;
            skipBtn.Pressed += Hide;

            InitializeTutorialPages();
        }

        private void InitializeTutorialPages()
        {
            _pages.Add("[b]欢迎来到杀戮尖塔![/b]\n\n这是一款卡牌构建 roguelike 游戏。\n\n你的目标是攀登尖塔，击败沿途的敌人。");
            _pages.Add("[b]卡牌战斗[/b]\n\n每回合你会抽取手牌，消耗能量打出卡牌。\n\n攻击卡造成伤害，技能卡提供各种效果。");
            _pages.Add("[b]地图导航[/b]\n\n选择你的路线，遭遇敌人、事件、商店和休息点。\n\n精英敌人更强但奖励更丰厚！");
            _pages.Add("[b]遗物与卡牌[/b]\n\n击败Boss获得遗物，提供永久增益。\n\n谨慎选择添加到牌组的卡牌！");

            ShowPage(0);
        }

        public void ShowTutorial()
        {
            _currentPage = 0;
            ShowPage(0);
            Show();
        }

        private void ShowPage(int index)
        {
            if (index >= 0 && index < _pages.Count && _contentLabel != null)
            {
                _contentLabel.Text = _pages[index];

                if (_nextButton != null)
                    _nextButton.Text = index == _pages.Count - 1 ? "完成" : "下一步";
            }
        }

        private void OnNextPressed()
        {
            _currentPage++;

            if (_currentPage >= _pages.Count)
            {
                Hide();
            }
            else
            {
                ShowPage(_currentPage);
            }
        }
    }
}
