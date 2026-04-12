using Godot;
using System;
using System.Collections.Generic;
using RoguelikeGame.Core;
using RoguelikeGame.Database;
using RoguelikeGame.Audio;

namespace RoguelikeGame.UI
{
    public partial class CharacterSelect : Control
    {
        private GridContainer _characterGrid;
        private Label _nameLabel;
        private Label _classLabel;
        private RichTextLabel _descLabel;
        private Button _confirmButton;
        private Button _backButton;

        private int _selectedIndex = -1;
        private readonly List<CharacterData> _characters = new();
        private readonly List<CharacterCardControl> _characterCards = new();

        [Signal]
        public delegate void CharacterSelectedEventHandler(string characterId);

        public override void _Ready()
        {
            SetupNodeReferences();
            SetupSignals();
            LoadCharacters();
            
            GD.Print("[CharacterSelect] Ready");
        }

        private void SetupNodeReferences()
        {
            _characterGrid = GetNodeOrNull<GridContainer>("CharGrid");
            _nameLabel = GetNodeOrNull<Label>("DescriptionPanel/DescVBox/NameLabel");
            _classLabel = GetNodeOrNull<Label>("DescriptionPanel/DescVBox/ClassLabel");
            _descLabel = GetNodeOrNull<RichTextLabel>("DescriptionPanel/DescVBox/DescLabel");
            _confirmButton = GetNodeOrNull<Button>("BottomBar/ConfirmButton");
            _backButton = GetNodeOrNull<Button>("HeaderBar/BackButton");

            if (_characterGrid == null)
                GD.PushError("[CharacterSelect] CharGrid not found");
            if (_descLabel == null)
                GD.PushError("[CharacterSelect] DescLabel not found");
        }

        private void SetupSignals()
        {
            if (_confirmButton != null)
            {
                _confirmButton.Pressed += OnConfirmPressed;
                _confirmButton.Disabled = true;
            }

            if (_backButton != null)
            {
                _backButton.Pressed += OnBackPressed;
            }
        }

        private void LoadCharacters()
        {
            var db = CharacterDatabase.Instance;
            if (db == null)
            {
                GD.PushError("[CharacterSelect] CharacterDatabase instance is null!");
                return;
            }

            _characters.AddRange(db.GetAllCharacters());
            GD.Print($"[CharacterSelect] Loaded {_characters.Count} characters");

            foreach (var child in _characterGrid.GetChildren())
            {
                child.QueueFree();
            }
            _characterCards.Clear();

            for (int i = 0; i < _characters.Count; i++)
            {
                var character = _characters[i];
                var card = new CharacterCardControl(i, character);
                card.CardClicked += OnCardClicked;
                _characterCards.Add(card);
                _characterGrid.AddChild(card);
            }

            UpdateDescription(-1);
        }

        private void OnCardClicked(int index)
        {
            GD.Print($"[CharacterSelect] Card clicked: {index}");
            SelectCharacter(index);
        }

        private void SelectCharacter(int index)
        {
            _selectedIndex = index;

            if (_confirmButton != null)
                _confirmButton.Disabled = false;

            foreach (var card in _characterCards)
            {
                card.SetSelected(card.Index == index);
            }

            UpdateDescription(index);
            AudioManager.Instance?.PlayButtonClick();
        }

        private void UpdateDescription(int index)
        {
            if (index < 0 || index >= _characters.Count)
            {
                if (_nameLabel != null) _nameLabel.Text = "选择一个角色";
                if (_classLabel != null) _classLabel.Text = "";
                if (_descLabel != null) _descLabel.Text = "点击上方角色卡片查看详情";
                return;
            }

            var character = _characters[index];

            if (_nameLabel != null)
                _nameLabel.Text = character.Name;

            if (_classLabel != null)
                _classLabel.Text = GetClassDisplayName(character.Class);

            if (_descLabel != null)
            {
                _descLabel.Text = $"{character.Title}\n\n{character.Description}\n\n难度: {character.DifficultyDescription}";
            }
        }

        private void OnConfirmPressed()
        {
            if (_selectedIndex >= 0 && _selectedIndex < _characters.Count)
            {
                var characterId = _characters[_selectedIndex].Id;
                GD.Print($"[CharacterSelect] Confirming character: {characterId}");
                
                AudioManager.Instance?.PlayButtonClick();
                EmitSignal(SignalName.CharacterSelected, characterId);
            }
        }

        private void OnBackPressed()
        {
            GD.Print("[CharacterSelect] Back pressed");
            AudioManager.Instance?.PlayButtonClick();
            Main.Instance?.GoToLobby();
        }

        private string GetClassDisplayName(CharacterClass cls) => cls switch
        {
            CharacterClass.Ironclad => "战士",
            CharacterClass.Silent => "猎人",
            CharacterClass.Defect => "机器人",
            CharacterClass.Watcher => "储君",
            CharacterClass.Necromancer => "死灵法师",
            _ => "未知"
        };
    }

    public partial class CharacterCardControl : PanelContainer
    {
        public int Index { get; }
        public CharacterData Data { get; }

        public event Action<int> CardClicked;

        private bool _selected;
        private TextureRect _portraitRect;
        private Label _nameLabel;

        public CharacterCardControl(int index, CharacterData data)
        {
            Index = index;
            Data = data;
        }

        public override void _Ready()
        {
            CustomMinimumSize = new Vector2(150, 180);
            MouseFilter = MouseFilterEnum.Stop;

            var style = new StyleBoxFlat
            {
                BgColor = new Color(0.12f, 0.1f, 0.08f),
                ContentMarginLeft = 10,
                ContentMarginRight = 10,
                ContentMarginTop = 10,
                ContentMarginBottom = 10,
                CornerRadiusTopLeft = 8,
                CornerRadiusTopRight = 8,
                CornerRadiusBottomLeft = 8,
                CornerRadiusBottomRight = 8,
                BorderWidthLeft = 2,
                BorderWidthRight = 2,
                BorderWidthTop = 2,
                BorderWidthBottom = 2,
                BorderColor = new Color(Data.BackgroundColor)
            };
            AddThemeStyleboxOverride("panel", style);

            var vbox = new VBoxContainer();
            AddChild(vbox);

            _portraitRect = new TextureRect
            {
                CustomMinimumSize = new Vector2(130, 110),
                ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                Modulate = new Color(0.5f, 0.5f, 0.5f)
            };
            
            if (!string.IsNullOrEmpty(Data.PortraitPath) && ResourceLoader.Exists(Data.PortraitPath))
            {
                _portraitRect.Texture = ResourceLoader.Load<Texture2D>(Data.PortraitPath);
                _portraitRect.Modulate = Colors.White;
            }
            vbox.AddChild(_portraitRect);

            _nameLabel = new Label
            {
                Text = Data.Name,
                HorizontalAlignment = HorizontalAlignment.Center,
                Modulate = Colors.White
            };
            vbox.AddChild(_nameLabel);

            GuiInput += OnGuiInput;
            MouseEntered += OnMouseEntered;
            MouseExited += OnMouseExited;
        }

        public void SetSelected(bool selected)
        {
            _selected = selected;
            
            var style = GetThemeStylebox("panel") as StyleBoxFlat;
            if (style != null)
            {
                style.BorderWidthLeft = selected ? 4 : 2;
                style.BorderWidthRight = selected ? 4 : 2;
                style.BorderWidthTop = selected ? 4 : 2;
                style.BorderWidthBottom = selected ? 4 : 2;
                style.BorderColor = selected ? Colors.Gold : new Color(Data.BackgroundColor);
            }

            if (selected)
            {
                var tween = CreateTween();
                tween.TweenProperty(this, "scale", new Vector2(1.05f, 1.05f), 0.1f);
            }
            else
            {
                var tween = CreateTween();
                tween.TweenProperty(this, "scale", Vector2.One, 0.1f);
            }
        }

        private void OnGuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
            {
                CardClicked?.Invoke(Index);
            }
        }

        private void OnMouseEntered()
        {
            if (!_selected)
            {
                var tween = CreateTween();
                tween.TweenProperty(this, "scale", new Vector2(1.02f, 1.02f), 0.1f);
            }
        }

        private void OnMouseExited()
        {
            if (!_selected)
            {
                var tween = CreateTween();
                tween.TweenProperty(this, "scale", Vector2.One, 0.1f);
            }
        }
    }
}
