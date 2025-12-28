using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Material.Icons;
using Material.Icons.Avalonia;

namespace ManaxClient.Controls;

public class IconButton : Button
{
    public static readonly StyledProperty<MaterialIconKind?> IconProperty =
        AvaloniaProperty.Register<IconButton, MaterialIconKind?>(nameof(Icon), MaterialIconKind.Error);

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<IconButton, string?>(nameof(Text));

    public IconButton()
    {
        MaterialIcon icon = new()
        {
            Width = 20,
            Height = 20,
            Margin = new Thickness(0, 0, 5, 0),
            [!MaterialIcon.KindProperty] = this[!IconProperty]
        };
        
        TextBlock textBlock = new()
        {
            VerticalAlignment = VerticalAlignment.Center
        };
        textBlock.Bind(TextBlock.TextProperty, this.GetObservable(TextProperty));
        this.GetObservable(TextProperty).Subscribe(text =>
        {
            icon.Margin = string.IsNullOrEmpty(text) 
                ? new Thickness(0) 
                : new Thickness(0, 0, 5, 0);
        });
        
        StackPanel stackPanel = new()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        stackPanel.Children.Add(icon);
        stackPanel.Children.Add(textBlock);
        
        Content = stackPanel;
        RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
        this[!BackgroundProperty] = new DynamicResourceExtension("MaterialCardBackgroundBrush");
        this[!BorderBrushProperty] = new DynamicResourceExtension("MaterialPrimaryLightBrush");
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
}