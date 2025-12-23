using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
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
        
        StackPanel stackPanel = new()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        stackPanel.Children.Add(iconImage);
        stackPanel.Children.Add(textBlock);
        
        Content = stackPanel;
        Padding= new Thickness(12, 6);
        BorderThickness = new Thickness(2);
        CornerRadius = new CornerRadius(5);
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Center;
        this[!BackgroundProperty] = new DynamicResourceExtension("MaterialCardBackgroundBrush");
        this[!BorderBrushProperty] = new DynamicResourceExtension("MaterialPrimaryLightBrush");
        
        AddHoverAnimation();
    }
    
    private void AddHoverAnimation()
    {
        RenderTransform = new ScaleTransform();
        RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
        Transitions scaleTransition =
        [
            new TransformOperationsTransition
            {
                Property = RenderTransformProperty,
                Duration = TimeSpan.FromMilliseconds(200),
                Easing = new CubicEaseOut()
            }
        ];
        Transitions = scaleTransition;
        
        PointerEntered += (_, _) =>
        {
            RenderTransform = new ScaleTransform(1.05, 1.05);
        };
        
        PointerExited += (_, _) =>
        {
            RenderTransform = new ScaleTransform(1.0, 1.0);
        };
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