using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using ManaxLibrary.DTO.Library;

namespace ManaxClient.Controls.Popups.Library;

public class LibraryCreatePopup() : ConfirmCancelPopup("Créer")
{
    private TextBox _nameBox = null!;
    private LibraryCreateDto? _result;

    protected override Grid GetFormGrid()
    {
        Grid grid = new()
        {
            RowSpacing = 16,
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto"),
            ColumnDefinitions = new ColumnDefinitions("*")
        };

        StackPanel headerPanel = new()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Margin = new Thickness(0, 0, 0, 8)
        };
        
        TextBlock icon = new()
        {
            Text = "📚",
            FontSize = 24,
            VerticalAlignment = VerticalAlignment.Center
        };
        
        StackPanel titleStack = new()
        {
            Orientation = Orientation.Vertical,
            Spacing = 4
        };
        
        TextBlock title = new()
        {
            Text = "Nouvelle bibliothèque",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#212529"))
        };
        
        TextBlock subtitle = new()
        {
            Text = "Créez une nouvelle bibliothèque pour organiser vos séries",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#6C757D"))
        };

        titleStack.Children.Add(title);
        titleStack.Children.Add(subtitle);
        headerPanel.Children.Add(icon);
        headerPanel.Children.Add(titleStack);
        
        Grid.SetRow(headerPanel, 0);
        grid.Children.Add(headerPanel);

        StackPanel inputPanel = new()
        {
            Spacing = 8
        };

        TextBlock nameLabel = new()
        {
            Text = "Nom de la bibliothèque",
            FontWeight = FontWeight.Medium,
            Foreground = new SolidColorBrush(Color.Parse("#495057"))
        };

        _nameBox = new TextBox
        {
            Watermark = "Entrez le nom de la bibliothèque...",
            Padding = new Thickness(12),
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.Parse("#DEE2E6")),
            FontSize = 14
        };

        _nameBox.PointerEntered += (s, _) =>
        {
            if (s is TextBox tb) tb.BorderBrush = new SolidColorBrush(Color.Parse("#007ACC"));
        };
        
        _nameBox.PointerExited += (s, _) =>
        {
            if (s is TextBox { IsFocused: false } tb) 
                tb.BorderBrush = new SolidColorBrush(Color.Parse("#DEE2E6"));
        };

        _nameBox.GotFocus += (s, _) =>
        {
            if (s is TextBox tb) tb.BorderBrush = new SolidColorBrush(Color.Parse("#007ACC"));
        };

        _nameBox.LostFocus += (s, _) =>
        {
            if (s is TextBox tb) tb.BorderBrush = new SolidColorBrush(Color.Parse("#DEE2E6"));
        };

        inputPanel.Children.Add(nameLabel);
        inputPanel.Children.Add(_nameBox);

        Grid.SetRow(inputPanel, 1);
        grid.Children.Add(inputPanel);

        Border helpBorder = new()
        {
            Background = new SolidColorBrush(Color.Parse("#E3F2FD")),
            BorderBrush = new SolidColorBrush(Color.Parse("#007ACC")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12)
        };

        StackPanel helpPanel = new()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8
        };

        TextBlock helpIcon = new()
        {
            Text = "💡",
            FontSize = 14,
            VerticalAlignment = VerticalAlignment.Top
        };

        TextBlock helpText = new()
        {
            Text = "Choisissez un nom descriptif pour votre bibliothèque. Vous pourrez l'organiser et la gérer facilement par la suite.",
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#495057"))
        };

        helpPanel.Children.Add(helpIcon);
        helpPanel.Children.Add(helpText);
        helpBorder.Child = helpPanel;

        Grid.SetRow(helpBorder, 2);
        grid.Children.Add(helpBorder);

        return grid;
    }

    protected override void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        string? name = _nameBox.Text?.Trim();
        if (string.IsNullOrEmpty(name)) return;
        _result = new LibraryCreateDto { Name = name };
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    public LibraryCreateDto? GetResult()
    {
        return _result;
    }
}