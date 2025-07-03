using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using ManaxApp.Models;

namespace ManaxApp.Controls;

public class SeriePreview : Button
{
    public static readonly AttachedProperty<ClientSerie> SerieProperty =
        AvaloniaProperty.RegisterAttached<SeriePreview, Grid, ClientSerie>(
            "Serie", new ClientSerie(), false, BindingMode.OneTime);

    public SeriePreview()
    {
        Width = 120;
        Height = Width * 1.61;

        Image image = new()
        {
            Stretch = Stretch.UniformToFill
        };
        image.Bind(Image.SourceProperty, new Binding("Serie.Poster")
        {
            Source = this,
            Mode = BindingMode.OneWay
        });
        image.SetValue(Grid.RowProperty, 0);


        TextBlock textBlock = new()
        {
            TextWrapping = TextWrapping.NoWrap,
            FontSize = 12
        };
        textBlock.Bind(TextBlock.TextProperty, new Binding("Serie.Info.Title")
        {
            Source = this,
            Mode = BindingMode.OneWay
        });
        textBlock.SetValue(Grid.RowProperty, 1);


        Grid grid = new()
        {
            RowDefinitions = new RowDefinitions("*, auto")
        };
        grid.Children.Add(image);
        grid.Children.Add(textBlock);

        Content = grid;
    }

    public ClientSerie Serie
    {
        get => GetSerie(this);
        set => SetSerie(this, value);
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