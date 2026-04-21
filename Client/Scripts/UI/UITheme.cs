using Godot;

namespace RoguelikeGame.Client.UI;

public static class UITheme
{
    private const string UI_BASE = "res://GameModes/base_game/Resources/UI/";
    private static readonly System.Collections.Generic.Dictionary<string, Texture2D> _cache = new();

    public static Texture2D GetIcon(string name)
    {
        if (_cache.TryGetValue(name, out var cached))
            return cached;

        var path = UI_BASE + name + ".png";
        if (!ResourceLoader.Exists(path))
            return null;

        var tex = ResourceLoader.Load(path) as Texture2D;
        if (tex != null)
            _cache[name] = tex;
        return tex;
    }

    private static StyleBoxTexture MakeStylebox(string texName,
        int marginLeft = 10, int marginRight = 10, int marginTop = 10, int marginBottom = 10,
        int contentLeft = 8, int contentRight = 8, int contentTop = 6, int contentBottom = 6)
    {
        var style = new StyleBoxTexture();
        var tex = GetIcon(texName);
        if (tex != null)
            style.Texture = tex;
        style.TextureMarginLeft = marginLeft;
        style.TextureMarginRight = marginRight;
        style.TextureMarginTop = marginTop;
        style.TextureMarginBottom = marginBottom;
        style.ContentMarginLeft = contentLeft;
        style.ContentMarginRight = contentRight;
        style.ContentMarginTop = contentTop;
        style.ContentMarginBottom = contentBottom;
        style.AxisStretchHorizontal = StyleBoxTexture.AxisStretchMode.Tile;
        style.AxisStretchVertical = StyleBoxTexture.AxisStretchMode.Tile;
        return style;
    }

    public static Button MakeButton(string text, string iconName = "", Vector2 minSize = default)
    {
        if (minSize == default) minSize = new Vector2(140, 40);
        var btn = new Button
        {
            CustomMinimumSize = minSize,
            MouseFilter = Control.MouseFilterEnum.Stop,
            Text = text
        };
        if (!string.IsNullOrEmpty(iconName))
        {
            btn.Icon = GetIcon(iconName);
            btn.IconAlignment = HorizontalAlignment.Left;
            btn.ExpandIcon = true;
        }
        btn.AddThemeStyleboxOverride("normal", MakeStylebox("btn_wide_normal", 10, 10, 8, 8, 8, 8, 4, 4));
        btn.AddThemeStyleboxOverride("hover", MakeStylebox("btn_wide_hover", 10, 10, 8, 8, 8, 8, 4, 4));
        btn.AddThemeStyleboxOverride("pressed", MakeStylebox("btn_wide_pressed", 10, 10, 8, 8, 8, 8, 4, 4));
        btn.AddThemeStyleboxOverride("disabled", MakeStylebox("btn_wide_disabled", 10, 10, 8, 8, 8, 8, 4, 4));
        btn.AddThemeColorOverride("font_color", new Color(0.95f, 0.9f, 0.8f));
        btn.AddThemeColorOverride("font_hover_color", new Color(1f, 1f, 1f));
        btn.AddThemeColorOverride("font_pressed_color", new Color(0.8f, 0.75f, 0.65f));
        btn.AddThemeColorOverride("font_disabled_color", new Color(0.5f, 0.45f, 0.4f, 0.6f));
        btn.AddThemeFontSizeOverride("font_size", 14);
        return btn;
    }

    public static Button MakeSmallButton(string text, string iconName = "", Vector2 minSize = default)
    {
        if (minSize == default) minSize = new Vector2(80, 32);
        var btn = new Button
        {
            CustomMinimumSize = minSize,
            MouseFilter = Control.MouseFilterEnum.Stop,
            Text = text
        };
        if (!string.IsNullOrEmpty(iconName))
        {
            btn.Icon = GetIcon(iconName);
            btn.IconAlignment = HorizontalAlignment.Left;
            btn.ExpandIcon = true;
        }
        btn.AddThemeStyleboxOverride("normal", MakeStylebox("btn_sq_s1_normal", 6, 6, 6, 6, 4, 4, 4, 4));
        btn.AddThemeStyleboxOverride("hover", MakeStylebox("btn_sq_s1_hover", 6, 6, 6, 6, 4, 4, 4, 4));
        btn.AddThemeStyleboxOverride("pressed", MakeStylebox("btn_sq_s1_pressed", 6, 6, 6, 6, 4, 4, 4, 4));
        btn.AddThemeStyleboxOverride("disabled", MakeStylebox("btn_sq_s1_disabled", 6, 6, 6, 6, 4, 4, 4, 4));
        btn.AddThemeColorOverride("font_color", new Color(0.95f, 0.9f, 0.8f));
        btn.AddThemeColorOverride("font_hover_color", new Color(1f, 1f, 1f));
        btn.AddThemeColorOverride("font_pressed_color", new Color(0.8f, 0.75f, 0.65f));
        btn.AddThemeFontSizeOverride("font_size", 12);
        return btn;
    }

    public static StyleBoxTexture MakePanelBg()
    {
        return MakeStylebox("panel_light", 12, 12, 12, 12, 10, 10, 8, 8);
    }

    public static StyleBoxTexture MakeDarkPanelBg()
    {
        return MakeStylebox("panel_dark", 14, 14, 12, 12, 12, 12, 8, 8);
    }

    public static StyleBoxTexture MakeWoodPanelBg()
    {
        return MakeStylebox("panel_wood", 6, 6, 6, 6, 4, 4, 4, 4);
    }

    public static StyleBoxTexture MakeMediumPanelBg()
    {
        return MakeStylebox("panel_medium", 14, 14, 12, 12, 12, 12, 8, 8);
    }

    public static StyleBoxTexture MakeCardPanelStyle()
    {
        return MakeStylebox("panel_card", 10, 10, 10, 10, 6, 6, 6, 6);
    }

    public static StyleBoxTexture MakeBarBgStyle()
    {
        return MakeStylebox("bar_bg", 8, 8, 8, 8, 2, 2, 2, 2);
    }

    public static TextureRect MakeIconRect(string name, Vector2 size = default)
    {
        if (size == default) size = new Vector2(24, 24);
        var rect = new TextureRect
        {
            Texture = GetIcon(name),
            CustomMinimumSize = size,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        return rect;
    }

    public static HBoxContainer MakeIconLabel(string iconName, string text, Vector2 iconSize = default)
    {
        if (iconSize == default) iconSize = new Vector2(20, 20);
        var hbox = new HBoxContainer();
        hbox.AddThemeConstantOverride("separation", 4);
        hbox.MouseFilter = Control.MouseFilterEnum.Ignore;
        hbox.AddChild(MakeIconRect(iconName, iconSize));
        var label = new Label
        {
            Text = text,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        hbox.AddChild(label);
        return hbox;
    }
}
