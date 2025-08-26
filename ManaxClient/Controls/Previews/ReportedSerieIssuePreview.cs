using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Issue.Reported;

namespace ManaxClient.Controls.Previews;

public class ReportedSerieIssuePreview : Button
{
    public static readonly AttachedProperty<ReportedIssueSerieDto> IssueProperty =
        AvaloniaProperty.RegisterAttached<ReportedSerieIssuePreview, Grid, ReportedIssueSerieDto>(
            "Issue", new ReportedIssueSerieDto(), false, BindingMode.OneTime);

    private readonly IBrush _backgroundColor = Brushes.White;
    private readonly IBrush _hoverColor = new SolidColorBrush(Color.Parse("#F8F9FA"));

    public ReportedSerieIssuePreview()
    {
        Background = Brushes.Transparent;
        BorderThickness = new Thickness(0);
        Padding = new Thickness(0);
        HorizontalAlignment = HorizontalAlignment.Stretch;
        HorizontalContentAlignment = HorizontalAlignment.Stretch;

        Border border = new()
        {
            Background = _backgroundColor,
            BorderBrush = new SolidColorBrush(Color.Parse("#FD7E14")),
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
            VerticalAlignment = VerticalAlignment.Center,
            Background = new SolidColorBrush(Color.Parse("#FD7E14"))
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
            Text = "Problème signalé pour une série",
            FontWeight = FontWeight.Medium,
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.Parse("#212529"))
        };

        TextBlock issueDetails = new()
        {
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#6C757D"))
        };

        issueDetails.Bind(TextBlock.TextProperty, new Binding("Issue")
        {
            Source = this,
            Mode = BindingMode.OneWay,
            Converter = new FuncValueConverter<ReportedIssueSerieDto, string>(issue =>
                issue != null
                    ? $"Série ID: {issue.SerieId} • Utilisateur ID: {issue.UserId} • {issue.CreatedAt:dd/MM/yyyy HH:mm}"
                    : string.Empty)
        });

        Border typeBadge = new()
        {
            Background = new SolidColorBrush(Color.Parse("#D1ECF1")),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(8, 4),
            VerticalAlignment = VerticalAlignment.Center
        };
        typeBadge.SetValue(Grid.ColumnProperty, 2);

        TextBlock typeText = new()
        {
            Text = "Signalé",
            FontSize = 11,
            FontWeight = FontWeight.Medium,
            Foreground = new SolidColorBrush(Color.Parse("#0C5460"))
        };

        typeBadge.Child = typeText;

        Button closeButton = new()
        {
            Content = "×",
            Background = new SolidColorBrush(Color.Parse("#FD7E14")),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(8),
            FontSize = 16,
            FontWeight = FontWeight.Bold,
            CornerRadius = new CornerRadius(4),
            VerticalAlignment = VerticalAlignment.Center,
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        closeButton.SetValue(Grid.ColumnProperty, 3);
        closeButton.Click += CloseIssue;

        infoStack.Children.Add(issueTitle);
        infoStack.Children.Add(issueDetails);

        mainGrid.Children.Add(statusIndicator);
        mainGrid.Children.Add(infoStack);
        mainGrid.Children.Add(typeBadge);
        mainGrid.Children.Add(closeButton);

        border.Child = mainGrid;
        Content = border;
    }

    public ReportedIssueSerieDto Issue
    {
        get => GetIssue(this);
        set => SetIssue(this, value);
    }

    private async void CloseIssue(object? sender, RoutedEventArgs e)
    {
        _ = await ManaxApiIssueClient.CloseSerieIssueAsync(Issue.Id);
        e.Handled = true;
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

    public static void SetIssue(AvaloniaObject element, ReportedIssueSerieDto issueValue)
    {
        element.SetValue(IssueProperty, issueValue);
    }

    public static ReportedIssueSerieDto GetIssue(AvaloniaObject element)
    {
        return element.GetValue(IssueProperty);
    }
}