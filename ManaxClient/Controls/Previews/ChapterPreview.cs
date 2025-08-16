using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using ManaxClient.Models;
using ManaxLibrary.DTO.Read;

namespace ManaxClient.Controls.Previews;

public class ChapterPreview : Button
{
    public static readonly AttachedProperty<ClientChapter> ChapterProperty =
        AvaloniaProperty.RegisterAttached<ChapterPreview, Grid, ClientChapter>(
            "Chapter", new ClientChapter(), false, BindingMode.OneTime);
    
    private IBrush _readTextColor = Brushes.Gray;
    private IBrush _unreadTextColor = Brushes.Black;
    private readonly IBrush _backgroundColor = Brushes.White;

    public ChapterPreview()
    {
        Background = Brushes.Transparent;
        Border border = new()
        {
            Margin = new Thickness(0, 1, 6, 1),
            Padding = new Thickness(2),
            BorderThickness = new Thickness(1),
            BorderBrush = Brushes.Black,
            CornerRadius = new CornerRadius(3),
            Background = _backgroundColor
        };
        
        StackPanel stackInfo = new()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5
        };

        TextBlock chapterName = new()
        {
            TextWrapping = TextWrapping.NoWrap,
            FontSize = 12
        };
        
        TextBlock pageRead = new()
        {
            FontSize = 12,
            Foreground = _unreadTextColor
        };
        
        chapterName.Bind(TextBlock.TextProperty, new Binding("Chapter.Info.FileName")
        {
            Source = this,
            Mode = BindingMode.OneWay,
            Converter = new FuncValueConverter<string, string>(fileName =>
                Path.GetFileNameWithoutExtension(fileName) ?? fileName ?? string.Empty)
        });
        
        chapterName.Bind(ForegroundProperty,new Binding("Chapter.Read")
        {
            Source = this,
            Mode = BindingMode.OneWay,
            Converter = new FuncValueConverter<ReadDto?, IBrush>(read =>
            {
                if (read == null) { return _unreadTextColor; }
                return read.Page+1 == Chapter.Info.Pages ? _readTextColor : _unreadTextColor;
            })
        });
        
        pageRead.Bind(TextBlock.TextProperty, new Binding("Chapter.Read")
        {
            Source = this,
            Mode = BindingMode.OneWay,
            Converter = new FuncValueConverter<ReadDto?, string>(page => page == null ? string.Empty : page.Page.ToString())
        });
        
        pageRead.Bind(IsVisibleProperty, new Binding("Chapter.Read")
        {
            Source = this,
            Mode = BindingMode.OneWay,
            Converter = new FuncValueConverter<ReadDto?, bool>(read =>
            {
                if (read == null) { return true; }
                return read.Page+1 != Chapter.Info.Pages;
            })
        });
        
        border.Child = stackInfo;
        stackInfo.Children.Add(chapterName);
        stackInfo.Children.Add(pageRead);
        Content = border;
    }

    public ClientChapter Chapter
    {
        get => GetChapter(this);
        set => SetChapter(this, value);
    }

    public static void SetChapter(AvaloniaObject element, ClientChapter serieValue)
    {
        element.SetValue(ChapterProperty, serieValue);
    }

    public static ClientChapter GetChapter(AvaloniaObject element)
    {
        return element.GetValue(ChapterProperty);
    }
}