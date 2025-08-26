using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using ManaxLibrary.DTO.Issue.Automatic;

namespace ManaxClient.Controls.Previews;

public class AutomaticChapterIssuePreview : Button
{
    public static readonly AttachedProperty<AutomaticIssueChapterDto> IssueProperty =
        AvaloniaProperty.RegisterAttached<AutomaticChapterIssuePreview, Grid, AutomaticIssueChapterDto>(
            "Issue", new AutomaticIssueChapterDto(), false, BindingMode.OneTime);

    private readonly IBrush _backgroundColor = Brushes.White;
    private readonly IBrush _hoverColor = new SolidColorBrush(Color.Parse("#F8F9FA"));

    public AutomaticChapterIssuePreview()
    {
        Background = Brushes.Transparent;
        BorderThickness = new Thickness(0);
        Padding = new Thickness(0);
        HorizontalAlignment = HorizontalAlignment.Stretch;
        HorizontalContentAlignment = HorizontalAlignment.Stretch;

        Border border = new()
        {
            Background = _backgroundColor,
            BorderBrush = new SolidColorBrush(Color.Parse("#DC3545")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16, 12)
        };

        Grid mainGrid = new()
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
            ColumnSpacing = 12
        };

        Border statusIndicator = new()
        {
            Width = 8,
            Height = 8,
            CornerRadius = new CornerRadius(4),
            VerticalAlignment = VerticalAlignment.Center,
            Background = new SolidColorBrush(Color.Parse("#DC3545"))
        };
        statusIndicator.SetValue(Grid.ColumnProperty, 0);

        StackPanel infoStack = new()
        {
            Orientation = Orientation.Vertical,
            Spacing = 4,
            VerticalAlignment = VerticalAlignment.Center
        };
        infoStack.SetValue(Grid.ColumnProperty, 1);

        TextBlock issueTitle = new()
        {
            FontWeight = FontWeight.Medium,
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.Parse("#212529"))
        };

        TextBlock issueDetails = new()
        {
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#6C757D"))
        };

        issueTitle.Bind(TextBlock.TextProperty, new Binding("Issue.Problem")
        {
            Source = this,
            Mode = BindingMode.OneWay,
            Converter = new FuncValueConverter<AutomaticIssueChapterType, string>(problem =>
                problem switch
                {
                    AutomaticIssueChapterType.ImageTooSmall => "Image trop petite",
                    AutomaticIssueChapterType.CouldNotOpen => "Impossible d'ouvrir",
                    AutomaticIssueChapterType.ChapterNumberMissing => "Numéro de chapitre manquant",
                    _ => problem.ToString()
                })
        });

        issueDetails.Bind(TextBlock.TextProperty, new Binding("Issue")
        {
            Source = this,
            Mode = BindingMode.OneWay,
            Converter = new FuncValueConverter<AutomaticIssueChapterDto, string>(issue =>
                issue != null ? $"Chapitre ID: {issue.ChapterId} • {issue.CreatedAt:dd/MM/yyyy HH:mm}" : string.Empty)
        });

        Border typeBadge = new()
        {
            Background = new SolidColorBrush(Color.Parse("#FFF3CD")),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(8, 4),
            VerticalAlignment = VerticalAlignment.Center
        };
        typeBadge.SetValue(Grid.ColumnProperty, 2);

        TextBlock typeText = new()
        {
            Text = "Automatique",
            FontSize = 11,
            FontWeight = FontWeight.Medium,
            Foreground = new SolidColorBrush(Color.Parse("#856404"))
        };

        typeBadge.Child = typeText;

        infoStack.Children.Add(issueTitle);
        infoStack.Children.Add(issueDetails);

        mainGrid.Children.Add(statusIndicator);
        mainGrid.Children.Add(infoStack);
        mainGrid.Children.Add(typeBadge);

        border.Child = mainGrid;
        Content = border;
    }

    public AutomaticIssueChapterDto Issue
    {
        get => GetIssue(this);
        set => SetIssue(this, value);
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        if (Content is Border border) border.Background = _hoverColor;
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        if (Content is Border border) border.Background = _backgroundColor;
    }

    public static void SetIssue(AvaloniaObject element, AutomaticIssueChapterDto issueValue)
    {
        element.SetValue(IssueProperty, issueValue);
    }

    public static AutomaticIssueChapterDto GetIssue(AvaloniaObject element)
    {
        return element.GetValue(IssueProperty);
    }
}