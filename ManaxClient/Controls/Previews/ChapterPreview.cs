using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using ManaxLibrary.DTOs.Chapter;

namespace ManaxClient.Controls.Previews;

public class ChapterPreview : Button
{
    public static readonly AttachedProperty<ChapterDTO> ChapterProperty =
        AvaloniaProperty.RegisterAttached<ChapterPreview, Grid, ChapterDTO>(
            "Chapter", new ChapterDTO(), false, BindingMode.OneTime);

    public ChapterPreview()
    {
        Background = Brushes.Transparent;
        Border border = new()
        {
            Margin = new Thickness(0,1,6,1),
            Padding = new Thickness(2),
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
            Mode = BindingMode.OneWay,
            Converter = new FuncValueConverter<string, string>(fileName => Path.GetFileNameWithoutExtension(fileName) ?? fileName ?? string.Empty)
        });
        border.Child = textBlock;
        Content = border;
    }

    public ChapterDTO Chapter
    {
        get => GetChapter(this);
        set => SetChapter(this, value);
    }

    public static void SetChapter(AvaloniaObject element, ChapterDTO serieValue)
    {
        element.SetValue(ChapterProperty, serieValue);
    }

    public static ChapterDTO GetChapter(AvaloniaObject element)
    {
        return element.GetValue(ChapterProperty);
    }
}