using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using ManaxLibrary.DTOs;

namespace ManaxApp.Controls;

public class SeriePreview : Grid
{
    public static readonly AttachedProperty<ClientSerie> SerieProperty = AvaloniaProperty.RegisterAttached<SeriePreview, Grid, ClientSerie>(
        "Serie", new ClientSerie(), false, BindingMode.OneTime);
    
    public static void SetSerie(AvaloniaObject element, ClientSerie serieValue)
    {
        element.SetValue(SerieProperty, serieValue);
    }
    
    public static ClientSerie GetSerie(AvaloniaObject element)
    {
        return element.GetValue(SerieProperty);
    }

    public ClientSerie Serie
    {
        get => GetSerie(this);
        set => SetSerie(this, value);
    }
    
    public SeriePreview()
    {
        Width = 120;
        Height = Width*1.61;
        RowDefinitions = new RowDefinitions("*, auto");
        
        Image image = new()
        {
            Stretch = Stretch.UniformToFill
        };
        image.Bind(Image.SourceProperty, new Binding("Serie.Poster")
        {
            Source = this,
            Mode = BindingMode.OneWay
        });
        image.SetValue(RowProperty, 0);
        Children.Add(image);
        
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
        textBlock.SetValue(RowProperty, 1);
        Children.Add(textBlock);
    }
}