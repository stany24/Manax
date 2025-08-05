using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using ManaxLibrary.DTOs.Rank;

namespace ManaxClient.Controls.Popups.Rank;

public class RankEditPopup(RankDTO rank) : ConfirmCancelPopup
{
    private TextBox _nameBox = null!;
    private NumericUpDown _valueBox = null!;

    protected override Grid GetFormGrid()
    {
        Grid grid = new()
        {
            Margin = new Thickness(10),
            RowSpacing = 5,
            RowDefinitions = new RowDefinitions("Auto,Auto"),
            ColumnDefinitions = new ColumnDefinitions("Auto,10,*")
        };

        TextBlock nameLabel = new()
        {
            Text = "Name:",
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(nameLabel, 0);
        Grid.SetColumn(nameLabel, 0);
        grid.Children.Add(nameLabel);

        _nameBox = new TextBox
        {
            Text = rank.Name
        };
        Grid.SetRow(_nameBox, 0);
        Grid.SetColumn(_nameBox, 2);
        grid.Children.Add(_nameBox);

        TextBlock valueLabel = new()
        {
            Text = "Value:",
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(valueLabel, 1);
        Grid.SetColumn(valueLabel, 0);
        grid.Children.Add(valueLabel);

        _valueBox = new NumericUpDown
        {
            Minimum = 0,
            Value = rank.Value,
            Increment = 1,
            FormatString = "0"
        };
        Grid.SetRow(_valueBox, 1);
        Grid.SetColumn(_valueBox, 2);
        grid.Children.Add(_valueBox);

        return grid;
    }

    protected override void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        string? name = _nameBox.Text?.Trim();
        int value = Convert.ToInt32(_valueBox.Value);
        if (string.IsNullOrEmpty(name)) return;
        rank.Value = value;
        rank.Name = name;
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    public RankDTO GetResult()
    {
        return rank;
    }
}