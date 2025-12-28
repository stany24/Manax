using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;

namespace ManaxClient.Controls;

public class IconButton : Button
{
    public static readonly StyledProperty<IImage?> IconProperty =
        AvaloniaProperty.Register<IconButton, IImage?>(nameof(Icon));

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<IconButton, string?>(nameof(Text));

    public IconButton()
    {
        Image iconImage = new()
        {
            Width = 20,
            Height = 20,
            Margin = new Thickness(0, 0, 5, 0)
        };
        iconImage.Bind(Image.SourceProperty, this.GetObservable(IconProperty));
        
        TextBlock textBlock = new()
        {
            VerticalAlignment = VerticalAlignment.Center
        };
        textBlock.Bind(TextBlock.TextProperty, this.GetObservable(TextProperty));
        this.GetObservable(TextProperty).Subscribe(text =>
        {
            iconImage.Margin = string.IsNullOrEmpty(text) 
                ? new Thickness(0) 
                : new Thickness(0, 0, 5, 0);
        });
        
        StackPanel stackPanel = new()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        stackPanel.Children.Add(iconImage);
        stackPanel.Children.Add(textBlock);
        
        Content = stackPanel;
        RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
        this[!BackgroundProperty] = new DynamicResourceExtension("MaterialCardBackgroundBrush");
        this[!BorderBrushProperty] = new DynamicResourceExtension("MaterialPrimaryLightBrush");
    }
    
    public IImage? Icon
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