using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using ManaxLibrary.DTO.SavePoint;

namespace ManaxClient.Controls.Popups.SavePoint;

public class SavePointCreatePopup() : ConfirmCancelPopup("Créer")
{
    private TextBox _pathBox = null!;
    private SavePointCreateDto? _result;

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
            Text = "💾",
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
            Text = "Nouveau point de sauvegarde",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#212529"))
        };

        TextBlock subtitle = new()
        {
            Text = "Définissez un répertoire de sauvegarde pour vos données",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#6C757D"))
        };

        titleStack.Children.Add(title);
        titleStack.Children.Add(subtitle);
        headerPanel.Children.Add(icon);
        headerPanel.Children.Add(titleStack);

        Grid.SetRow(headerPanel, 0);
        grid.Children.Add(headerPanel);

        StackPanel pathPanel = new()
        {
            Spacing = 8
        };

        TextBlock pathLabel = new()
        {
            Text = "Chemin de sauvegarde",
            FontWeight = FontWeight.Medium,
            Foreground = new SolidColorBrush(Color.Parse("#495057"))
        };

        Grid pathInputGrid = new()
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            ColumnSpacing = 8
        };

        _pathBox = new TextBox
        {
            Watermark = "Entrez le chemin du répertoire...",
            Padding = new Thickness(12),
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.Parse("#DEE2E6")),
            FontSize = 14
        };
        Grid.SetColumn(_pathBox, 0);

        Button browseButton = new()
        {
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Children =
                {
                    new TextBlock { Text = "📁", FontSize = 14 },
                    new TextBlock { Text = "Parcourir" }
                }
            },
            Background = new SolidColorBrush(Color.Parse("#E9ECEF")),
            Foreground = new SolidColorBrush(Color.Parse("#495057")),
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12, 8)
        };
        Grid.SetColumn(browseButton, 1);

        pathInputGrid.Children.Add(_pathBox);
        pathInputGrid.Children.Add(browseButton);

        pathPanel.Children.Add(pathLabel);
        pathPanel.Children.Add(pathInputGrid);
        Grid.SetRow(pathPanel, 1);
        grid.Children.Add(pathPanel);

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
            Text =
                "Sélectionnez un répertoire accessible où vos données seront sauvegardées automatiquement. Assurez-vous d'avoir les permissions d'écriture.",
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#495057"))
        };

        helpPanel.Children.Add(helpIcon);
        helpPanel.Children.Add(helpText);
        helpBorder.Child = helpPanel;

        Grid.SetRow(helpBorder, 2);
        grid.Children.Add(helpBorder);

        ApplyInputStyles(_pathBox);

        browseButton.PointerEntered += (s, _) =>
        {
            if (s is Button btn) btn.Background = new SolidColorBrush(Color.Parse("#DEE2E6"));
        };
        browseButton.PointerExited += (s, _) =>
        {
            if (s is Button btn) btn.Background = new SolidColorBrush(Color.Parse("#E9ECEF"));
        };

        return grid;
    }

    private static void ApplyInputStyles(Control control)
    {
        control.PointerEntered += (s, _) =>
        {
            if (s is Control c) SetBorderBrush(c, new SolidColorBrush(Color.Parse("#007ACC")));
        };

        control.PointerExited += (s, _) =>
        {
            if (s is Control { IsFocused: false } c)
                SetBorderBrush(c, new SolidColorBrush(Color.Parse("#DEE2E6")));
        };

        control.GotFocus += (s, _) =>
        {
            if (s is Control c) SetBorderBrush(c, new SolidColorBrush(Color.Parse("#007ACC")));
        };

        control.LostFocus += (s, _) =>
        {
            if (s is Control c) SetBorderBrush(c, new SolidColorBrush(Color.Parse("#DEE2E6")));
        };
    }

    private static void SetBorderBrush(Control control, IBrush brush)
    {
        if (control is TextBox tb) tb.BorderBrush = brush;
    }

    protected override void OkButtonClicked(object? sender, RoutedEventArgs e)
    {
        string? path = _pathBox.Text?.Trim();
        if (string.IsNullOrEmpty(path)) return;
        _result = new SavePointCreateDto { Path = path };
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    public SavePointCreateDto? GetResult()
    {
        return _result;
    }
}