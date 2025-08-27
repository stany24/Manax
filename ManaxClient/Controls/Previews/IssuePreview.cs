using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

namespace ManaxClient.Controls.Previews;

public class IssuePreview : Button
{
    public static readonly StyledProperty<string> ProblemProperty =
        AvaloniaProperty.Register<IssuePreview, string>(nameof(Problem), string.Empty);

    public static readonly StyledProperty<string> InfoProperty =
        AvaloniaProperty.Register<IssuePreview, string>(nameof(Info), string.Empty);

    public static readonly StyledProperty<bool> ClosableProperty =
        AvaloniaProperty.Register<IssuePreview, bool>(nameof(Closable));

    public static readonly StyledProperty<ICommand?> CloseCommandProperty =
        AvaloniaProperty.Register<IssuePreview, ICommand?>(nameof(CloseCommand));

    public static readonly StyledProperty<IBrush> IssueColorProperty =
        AvaloniaProperty.Register<IssuePreview, IBrush>(nameof(IssueColor),
            new SolidColorBrush(Color.Parse("#DC3545")));

    public static readonly StyledProperty<string> BadgeTextProperty =
        AvaloniaProperty.Register<IssuePreview, string>(nameof(BadgeText), "Issue");

    private readonly IBrush _backgroundColor = Brushes.White;
    private readonly IBrush _hoverColor = new SolidColorBrush(Color.Parse("#F8F9FA"));
    private Button _closeButton;
    private TextBlock _infoText;
    private TextBlock _problemText;

    public IssuePreview()
    {
        Background = Brushes.Transparent;
        BorderThickness = new Thickness(0);
        Padding = new Thickness(0);
        HorizontalAlignment = HorizontalAlignment.Stretch;
        HorizontalContentAlignment = HorizontalAlignment.Stretch;

        InitializeComponent();
        BindProperties();
    }

    public string Problem
    {
        get => GetValue(ProblemProperty);
        set => SetValue(ProblemProperty, value);
    }

    public string Info
    {
        get => GetValue(InfoProperty);
        set => SetValue(InfoProperty, value);
    }

    public bool Closable
    {
        get => GetValue(ClosableProperty);
        set => SetValue(ClosableProperty, value);
    }

    public ICommand? CloseCommand
    {
        get => GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    public IBrush IssueColor
    {
        get => GetValue(IssueColorProperty);
        set => SetValue(IssueColorProperty, value);
    }

    public string BadgeText
    {
        get => GetValue(BadgeTextProperty);
        set => SetValue(BadgeTextProperty, value);
    }

    private void InitializeComponent()
    {
        Border border = new()
        {
            Background = _backgroundColor,
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

        StackPanel infoStack = new()
        {
            Orientation = Orientation.Vertical,
            Spacing = 4,
            VerticalAlignment = VerticalAlignment.Center
        };
        infoStack.SetValue(Grid.ColumnProperty, 1);

        _problemText = new TextBlock
        {
            FontWeight = FontWeight.Medium,
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.Parse("#212529"))
        };

        _infoText = new TextBlock
        {
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#6C757D"))
        };

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
            FontSize = 11,
            FontWeight = FontWeight.Medium,
            Foreground = new SolidColorBrush(Color.Parse("#856404"))
        };
        typeText.Bind(TextBlock.TextProperty, new Binding(nameof(BadgeText)) { Source = this });

        _closeButton = new Button
        {
            Content = "×",
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(8),
            FontSize = 16,
            FontWeight = FontWeight.Bold,
            CornerRadius = new CornerRadius(4),
            VerticalAlignment = VerticalAlignment.Center,
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        _closeButton.SetValue(Grid.ColumnProperty, 3);
        _closeButton.Click += OnCloseClick;

        typeBadge.Child = typeText;

        infoStack.Children.Add(_problemText);
        infoStack.Children.Add(_infoText);

        mainGrid.Children.Add(statusIndicator);
        mainGrid.Children.Add(infoStack);
        mainGrid.Children.Add(typeBadge);
        mainGrid.Children.Add(_closeButton);

        border.Child = mainGrid;
        Content = border;

        border.Bind(Border.BorderBrushProperty, new Binding(nameof(IssueColor)) { Source = this });
        statusIndicator.Bind(Border.BackgroundProperty, new Binding(nameof(IssueColor)) { Source = this });
        _closeButton.Bind(BackgroundProperty, new Binding(nameof(IssueColor)) { Source = this });
    }

    private void BindProperties()
    {
        _problemText.Bind(TextBlock.TextProperty, new Binding(nameof(Problem)) { Source = this });
        _infoText.Bind(TextBlock.TextProperty, new Binding(nameof(Info)) { Source = this });
        _closeButton.Bind(IsVisibleProperty, new Binding(nameof(Closable)) { Source = this });
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        CloseCommand?.Execute(null);
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
}