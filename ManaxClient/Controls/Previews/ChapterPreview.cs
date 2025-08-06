using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using ManaxLibrary.DTO.Chapter;

namespace ManaxClient.Controls.Previews;

public class ChapterPreview : Button
{
    public static readonly AttachedProperty<ChapterDto> ChapterProperty =
        AvaloniaProperty.RegisterAttached<ChapterPreview, Grid, ChapterDto>(
            "Chapter", new ChapterDto(), false, BindingMode.OneTime);

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

    public ChapterDto Chapter
    {
        get => GetChapter(this);
        set => SetChapter(this, value);
    }

    public static void SetChapter(AvaloniaObject element, ChapterDto serieValue)
    {
        element.SetValue(ChapterProperty, serieValue);
    }

    public static ChapterDto GetChapter(AvaloniaObject element)
    {
        return element.GetValue(ChapterProperty);
    }
}