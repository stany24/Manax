using System.IO;
using Avalonia;
using Avalonia.Animation;
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
    
    private readonly IBrush _readTextColor = new SolidColorBrush(Color.Parse("#6C757D"));
    private readonly IBrush _unreadTextColor = new SolidColorBrush(Color.Parse("#212529"));
    private readonly IBrush _backgroundColor = Brushes.White;
    private readonly IBrush _hoverColor = new SolidColorBrush(Color.Parse("#F8F9FA"));

    public ChapterPreview()
    {
        Background = Brushes.Transparent;
        BorderThickness = new Thickness(0);
        Padding = new Thickness(0);
        HorizontalAlignment = HorizontalAlignment.Stretch;
        HorizontalContentAlignment = HorizontalAlignment.Stretch;

        Border border = new()
        {
            Background = _backgroundColor,
            BorderBrush = new SolidColorBrush(Color.Parse("#E9ECEF")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16, 12)
        };
        
        Grid mainGrid = new()
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto,Auto"),
            ColumnSpacing = 12
        };

        Border statusIndicator = new()
        {
            Width = 8,
            Height = 8,
            CornerRadius = new CornerRadius(4),
            VerticalAlignment = VerticalAlignment.Center
        };
        statusIndicator.SetValue(Grid.ColumnProperty, 0);
        
        statusIndicator.Bind(Border.BackgroundProperty, new Binding("Chapter.Read")
        {
            Source = this,
            Mode = BindingMode.OneWay,
            Converter = new FuncValueConverter<ReadDto?, IBrush>(read =>
            {
                if (read == null) return new SolidColorBrush(Color.Parse("#6C757D"));
                return read.Page + 1 == Chapter.Info.Pages 
                    ? new SolidColorBrush(Color.Parse("#28A745"))
                    : new SolidColorBrush(Color.Parse("#007ACC"));
            })
        });

        StackPanel infoStack = new()
        {
            Orientation = Orientation.Vertical,
            Spacing = 4,
            VerticalAlignment = VerticalAlignment.Center
        };
        infoStack.SetValue(Grid.ColumnProperty, 1);

        TextBlock chapterName = new()
        {
            FontWeight = FontWeight.Medium,
            FontSize = 14,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        
        TextBlock chapterDetails = new()
        {
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#6C757D"))
        };
        
        chapterName.Bind(TextBlock.TextProperty, new Binding("Chapter.Info.FileName")
        {
            Source = this,
            Mode = BindingMode.OneWay,
            Converter = new FuncValueConverter<string, string>(fileName =>
                Path.GetFileNameWithoutExtension(fileName) ?? fileName ?? string.Empty)
        });
        
        chapterName.Bind(ForegroundProperty, new Binding("Chapter.Read")
        {
            Source = this,
            Mode = BindingMode.OneWay,
            Converter = new FuncValueConverter<ReadDto?, IBrush>(read =>
            {
                if (read == null) return _unreadTextColor;
                return read.Page + 1 == Chapter.Info.Pages ? _readTextColor : _unreadTextColor;
            })
        });
        
        chapterDetails.Bind(TextBlock.TextProperty, new Binding("Chapter.Info.Pages")
        {
            Source = this,
            Mode = BindingMode.OneWay,
            Converter = new FuncValueConverter<int, string>(pages => $"{pages} page(s)")
        });

        Border progressBadge = new()
        {
            Background = new SolidColorBrush(Color.Parse("#E9ECEF")),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(8, 4),
            VerticalAlignment = VerticalAlignment.Center
        };
        progressBadge.SetValue(Grid.ColumnProperty, 2);
        
        TextBlock progressText = new()
        {
            FontSize = 11,
            FontWeight = FontWeight.Medium,
            Foreground = new SolidColorBrush(Color.Parse("#495057"))
        };
        
        progressText.Bind(TextBlock.TextProperty, new Binding("Chapter.Read")
        {
            Source = this,
            Mode = BindingMode.OneWay,
            Converter = new FuncValueConverter<ReadDto?, string>(read =>
            {
                if (read == null) return "Non lu";
                int currentPage = read.Page + 1;
                int totalPages = Chapter.Info.Pages;
                return currentPage == totalPages ? "Terminé" : $"{currentPage}/{totalPages}";
            })
        });
        
        progressBadge.Child = progressText;
        
        progressBadge.Bind(Border.BackgroundProperty, new Binding("Chapter.Read")
        {
            Source = this,
            Mode = BindingMode.OneWay,
            Converter = new FuncValueConverter<ReadDto?, IBrush>(read =>
            {
                if (read == null) return new SolidColorBrush(Color.Parse("#E9ECEF"));
                return read.Page + 1 == Chapter.Info.Pages 
                    ? new SolidColorBrush(Color.Parse("#D4EDDA"))
                    : new SolidColorBrush(Color.Parse("#CCE5FF"));
            })
        });

        TextBlock chevron = new()
        {
            Text = "›",
            FontSize = 16,
            Foreground = new SolidColorBrush(Color.Parse("#6C757D")),
            VerticalAlignment = VerticalAlignment.Center
        };
        chevron.SetValue(Grid.ColumnProperty, 3);

        infoStack.Children.Add(chapterName);
        infoStack.Children.Add(chapterDetails);
        
        mainGrid.Children.Add(statusIndicator);
        mainGrid.Children.Add(infoStack);
        mainGrid.Children.Add(progressBadge);
        mainGrid.Children.Add(chevron);
        
        border.Child = mainGrid;
        Content = border;

        Transitions =
        [
            new BrushTransition
            {
                Property = BackgroundProperty,
                Duration = System.TimeSpan.FromMilliseconds(150)
            }
        ];
    }

    protected override void OnPointerEntered(Avalonia.Input.PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        if (Content is Border border)
        {
            border.Background = _hoverColor;
        }
    }

    protected override void OnPointerExited(Avalonia.Input.PointerEventArgs e)
    {
        base.OnPointerExited(e);
        if (Content is Border border)
        {
            border.Background = _backgroundColor;
        }
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