using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using ManaxClient.Models;

namespace ManaxClient.Controls.Previews;

public class SeriePreview : Button
{
    public static readonly AttachedProperty<ClientSerie?> SerieProperty =
        AvaloniaProperty.RegisterAttached<SeriePreview, Grid, ClientSerie?>(
            "Serie", null, false, BindingMode.OneTime);

    public static readonly AttachedProperty<SolidColorBrush> TextColorProperty =
        AvaloniaProperty.RegisterAttached<SeriePreview, Grid, SolidColorBrush>(
            "TextColor", new SolidColorBrush(), false, BindingMode.OneTime);

    public static readonly AttachedProperty<SolidColorBrush> BackGroundColorProperty =
        AvaloniaProperty.RegisterAttached<SeriePreview, Grid, SolidColorBrush>(
            "BackGroundColor", new SolidColorBrush(), false, BindingMode.OneTime);

    public SeriePreview()
    {
        Width = 150;
        Height = Width * 1.6;
        Padding = new Thickness(0);
        Background = Brushes.White;
        BorderThickness = new Thickness(0);
        CornerRadius = new CornerRadius(12);

        Image image = new()
        {
            Stretch = Stretch.UniformToFill,
            ClipToBounds = true
        };
        image.Bind(Image.SourceProperty, new Binding("Serie.Poster")
        {
            Source = this,
            Mode = BindingMode.OneWay
        });
        image.SetValue(Grid.RowProperty, 0);

        Border imageBorder = new()
        {
            Child = image,
            CornerRadius = new CornerRadius(12, 12, 0, 0),
            ClipToBounds = true
        };

        Border titleContainer = new()
        {
            Background = Brushes.Transparent,
            Padding = new Thickness(12, 8),
            CornerRadius = new CornerRadius(0, 0, 12, 12),
            VerticalAlignment = VerticalAlignment.Bottom
        };

        titleContainer.Bind(Border.BackgroundProperty, new Binding("BackGroundColor")
        {
            Source = this,
            Mode = BindingMode.OneWay,
            Converter = new FuncValueConverter<SolidColorBrush, LinearGradientBrush>(brush => new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                GradientStops =
                [
                    new GradientStop(brush?.Color ?? Colors.White, 0),
                    new GradientStop(Colors.Transparent, 1)
                ]
            })
        });

        TextBlock textBlock = new()
        {
            TextWrapping = TextWrapping.Wrap,
            FontSize = 14,
            FontWeight = FontWeight.Medium,
            TextAlignment = TextAlignment.Left,
            MaxLines = 2,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        textBlock.Bind(TextBlock.TextProperty, new Binding("Serie.Info.Title")
        {
            Source = this,
            Mode = BindingMode.OneWay
        });
        textBlock.Bind(TextBlock.ForegroundProperty, new Binding("TextColor")
        {
            Source = this,
            Mode = BindingMode.OneWay
        });

        titleContainer.Child = textBlock;

        Panel grid = new()
        {
            ClipToBounds = true
        };
        grid.Children.Add(imageBorder);
        grid.Children.Add(titleContainer);

        Border outerBorder = new()
        {
            Child = grid,
            CornerRadius = new CornerRadius(12),
            ClipToBounds = true
        };

        Content = outerBorder;
    }

    public ClientSerie? Serie
    {
        get => GetSerie(this);
        set => SetSerie(this, value);
    }

    public SolidColorBrush TextColor
    {
        get => GetTextColor(this);
        set => SetTextColor(this, value);
    }

    public SolidColorBrush BackGroundColor
    {
        get => GetBackGroundColor(this);
        set => SetBackGroundColor(this, value);
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        RenderTransform = new ScaleTransform(1.05, 1.05);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        RenderTransform = new ScaleTransform(1.0, 1.0);
    }

    private static void SetSerie(AvaloniaObject element, ClientSerie? serieValue)
    {
        element.SetValue(SerieProperty, serieValue);
    }

    private static ClientSerie? GetSerie(AvaloniaObject element)
    {
        return element.GetValue(SerieProperty);
    }

    private static void SetTextColor(AvaloniaObject element, SolidColorBrush textColorValue)
    {
        element.SetValue(TextColorProperty, textColorValue);
    }

    private static SolidColorBrush GetTextColor(AvaloniaObject element)
    {
        return element.GetValue(TextColorProperty);
    }

    private static void SetBackGroundColor(AvaloniaObject element, SolidColorBrush backGroundColorValue)
    {
        element.SetValue(BackGroundColorProperty, backGroundColorValue);
    }

    private static SolidColorBrush GetBackGroundColor(AvaloniaObject element)
    {
        return element.GetValue(BackGroundColorProperty);
    }
}