using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using ManaxLibrary.DTO.Rank;

namespace ManaxClient.Controls.Popups.Rank;

public class RankEditPopup(RankDto rank) : ConfirmCancelPopup("Modifier")
{
    private TextBox _nameBox = null!;
    private NumericUpDown _valueBox = null!;

    protected override Grid GetFormGrid()
    {
        Grid grid = new()
        {
            RowSpacing = 16,
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto,Auto"),
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
            Text = "🏆",
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
            Text = "Modifier le rang",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#212529"))
        };
        
        TextBlock subtitle = new()
        {
            Text = "Modifiez les informations de ce rang utilisateur",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#6C757D"))
        };

        titleStack.Children.Add(title);
        titleStack.Children.Add(subtitle);
        headerPanel.Children.Add(icon);
        headerPanel.Children.Add(titleStack);
        
        Grid.SetRow(headerPanel, 0);
        grid.Children.Add(headerPanel);

        StackPanel namePanel = new()
        {
            Spacing = 8
        };

        TextBlock nameLabel = new()
        {
            Text = "Nom du rang",
            FontWeight = FontWeight.Medium,
            Foreground = new SolidColorBrush(Color.Parse("#495057"))
        };

        _nameBox = new TextBox
        {
            Text = rank.Name,
            Watermark = "Entrez le nom du rang...",
            Padding = new Thickness(12),
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.Parse("#DEE2E6")),
            FontSize = 14
        };

        namePanel.Children.Add(nameLabel);
        namePanel.Children.Add(_nameBox);
        Grid.SetRow(namePanel, 1);
        grid.Children.Add(namePanel);

        StackPanel valuePanel = new()
        {
            Spacing = 8
        };

        TextBlock valueLabel = new()
        {
            Text = "Valeur du rang",
            FontWeight = FontWeight.Medium,
            Foreground = new SolidColorBrush(Color.Parse("#495057"))
        };

        _valueBox = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 1000,
            Value = rank.Value,
            Increment = 1,
            FormatString = "0",
            Padding = new Thickness(12),
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.Parse("#DEE2E6")),
            FontSize = 14,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        valuePanel.Children.Add(valueLabel);
        valuePanel.Children.Add(_valueBox);
        Grid.SetRow(valuePanel, 2);
        grid.Children.Add(valuePanel);

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
            Text = "La valeur détermine la hiérarchie des rangs. Plus la valeur est élevée, plus le rang est important.",
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#495057"))
        };

        helpPanel.Children.Add(helpIcon);
        helpPanel.Children.Add(helpText);
        helpBorder.Child = helpPanel;

        Grid.SetRow(helpBorder, 3);
        grid.Children.Add(helpBorder);

        ApplyInputStyles(_nameBox);
        ApplyInputStyles(_valueBox);

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
        switch (control)
        {
            case TextBox tb:
                tb.BorderBrush = brush;
                break;
            case NumericUpDown nud:
                nud.BorderBrush = brush;
                break;
        }
    }

    protected override void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        string? name = _nameBox.Text?.Trim();
        int value = Convert.ToInt32(_valueBox.Value);
        if (string.IsNullOrEmpty(name)) return;
        rank.Value = value;
        rank.Name = name;
        Canceled = false;
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    public RankDto GetResult()
    {
        return rank;
    }
}