using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using ManaxLibrary.DTOs;

namespace ManaxApp.Controls;

public class ChapterPreview : Button
{
    public static readonly AttachedProperty<ChapterDTO> ChapterProperty = AvaloniaProperty.RegisterAttached<ChapterPreview, Grid, ChapterDTO>(
        "Chapter", new ChapterDTO(), false, BindingMode.OneTime);
    
    public static void SetChapter(AvaloniaObject element, ChapterDTO serieValue)
    {
        element.SetValue(ChapterProperty, serieValue);
    }
    
    public static ChapterDTO GetChapter(AvaloniaObject element)
    {
        return element.GetValue(ChapterProperty);
    }

    public ChapterDTO Chapter
    {
        get => GetChapter(this);
        set => SetChapter(this, value);
    }
    
    public ChapterPreview()
    {
        Border border = new()
        {
            BorderThickness = new Thickness(1),
            BorderBrush = Brushes.Black,
            CornerRadius = new CornerRadius(3)
        };

        TextBlock textBlock = new()
        {
            TextWrapping = TextWrapping.NoWrap,
            FontSize = 12
        };
        textBlock.Bind(TextBlock.TextProperty, new Binding("Chapter.FileName")
        {
            Source = this,
            Mode = BindingMode.OneWay
        });
        border.Child = textBlock;
        Content = border;
    }
}