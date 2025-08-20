using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using ManaxClient.Models;

namespace ManaxClient.Controls.Previews;

public class SeriePreview : Button
{
    public static readonly AttachedProperty<ClientSerie> SerieProperty =
        AvaloniaProperty.RegisterAttached<SeriePreview, Grid, ClientSerie>(
            "Serie", new ClientSerie(), false, BindingMode.OneTime);

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
        imageBorder.SetValue(Grid.RowProperty, 0);

        Border titleContainer = new()
        {
            Background = Brushes.White,
            Padding = new Thickness(12, 8),
            CornerRadius = new CornerRadius(0, 0, 12, 12)
        };

        TextBlock textBlock = new()
        {
            TextWrapping = TextWrapping.Wrap,
            FontSize = 13,
            FontWeight = FontWeight.Medium,
            Foreground = new SolidColorBrush(Color.Parse("#212529")),
            TextAlignment = TextAlignment.Center,
            MaxLines = 2,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        textBlock.Bind(TextBlock.TextProperty, new Binding("Serie.Info.Title")
        {
            Source = this,
            Mode = BindingMode.OneWay
        });

        titleContainer.Child = textBlock;
        titleContainer.SetValue(Grid.RowProperty, 1);

        Grid grid = new()
        {
            RowDefinitions = new RowDefinitions("*, Auto"),
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

    public ClientSerie Serie
    {
        get => GetSerie(this);
        set => SetSerie(this, value);
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

    public static void SetSerie(AvaloniaObject element, ClientSerie serieValue)
    {
        element.SetValue(SerieProperty, serieValue);
    }

    public static ClientSerie GetSerie(AvaloniaObject element)
    {
        return element.GetValue(SerieProperty);
    }
}