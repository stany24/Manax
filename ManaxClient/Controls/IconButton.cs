using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Material.Icons;
using Material.Icons.Avalonia;

namespace ManaxClient.Controls;

public class IconButton : Button
{
    public static readonly StyledProperty<MaterialIconKind?> IconProperty =
        AvaloniaProperty.Register<IconButton, MaterialIconKind?>(nameof(Icon), MaterialIconKind.Error);

    public static readonly StyledProperty<IBrush?> IconColorProperty =
        AvaloniaProperty.Register<IconButton, IBrush?>(nameof(Icon));

    public static readonly StyledProperty<bool> ShowTextProperty =
        AvaloniaProperty.Register<IconButton, bool>(nameof(ShowText), true);

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<IconButton, string?>(nameof(Text));

    private readonly MaterialIcon _icon;

    public IconButton()
    {
        _icon = new MaterialIcon
        {
            Width = 20,
            Height = 20,
            Margin = new Thickness(0, 0, 5, 0),
            [!MaterialIcon.KindProperty] = this[!IconProperty],
            [!ForegroundProperty] = this[!IconColorProperty]
        };

        TextBlock textBlock = new()
        {
            VerticalAlignment = VerticalAlignment.Center,
            [!IsVisibleProperty] = this[!ShowTextProperty]
        };
        textBlock.Bind(TextBlock.TextProperty, this.GetObservable(TextProperty));

        this.GetObservable(TextProperty).Subscribe(_ => UpdateMargin());
        this.GetObservable(ShowTextProperty).Subscribe(_ => UpdateMargin());

        StackPanel stackPanel = new()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        stackPanel.Children.Add(_icon);
        stackPanel.Children.Add(textBlock);

        Content = stackPanel;
        RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
    }

    public IBrush? IconColor
    {
        get => GetValue(IconColorProperty);
        set => SetValue(IconColorProperty, value);
    }

    public MaterialIconKind? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool ShowText
    {
        get => GetValue(ShowTextProperty);
        set => SetValue(ShowTextProperty, value);
    }

    private void UpdateMargin()
    {
        bool showText = GetValue(ShowTextProperty);
        string? text = GetValue(TextProperty);
        _icon.Margin = showText && !string.IsNullOrEmpty(text)
            ? new Thickness(0, 0, 5, 0)
            : new Thickness(0);
    }
}