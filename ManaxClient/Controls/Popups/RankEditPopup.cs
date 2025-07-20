using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using ManaxLibrary.DTOs.Rank;

namespace ManaxClient.Controls.Popups;

public class RankEditPopup : Popup
{
    private readonly Button _cancelButton;
    private readonly TextBox _nameBox;
    private readonly Button _okButton;
    private readonly NumericUpDown _valueBox;
    private readonly RankDTO _result;

    public RankEditPopup(RankDTO rank)
    {
        _result = rank;
        MinWidth = 200;
        MaxWidth = 500;
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Center;

        Grid grid = new()
        {
            Margin = new Thickness(10),
            RowSpacing = 5,
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto"),
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
            Text = rank.Name,
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

        Grid buttonGrid = new()
        {
            ColumnDefinitions = new ColumnDefinitions("*,10,*")
        };
        _okButton = new Button
        {
            HotKey = new KeyGesture(Key.Enter),
            Content = "Confirm",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };
        Grid.SetColumn(_okButton, 2);
        _okButton.Click += OkButton_Click;

        _cancelButton = new Button
        {
            Content = "Cancel",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };
        Grid.SetColumn(_cancelButton, 0);
        _cancelButton.Click += (_,_) => Close();

        buttonGrid.Children.Add(_okButton);
        buttonGrid.Children.Add(_cancelButton);
        Grid.SetRow(buttonGrid, 3);
        Grid.SetColumn(buttonGrid, 0);
        Grid.SetColumnSpan(buttonGrid, 3);
        grid.Children.Add(buttonGrid);

        Form.Content = grid;
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        string? name = _nameBox.Text?.Trim();
        int value = Convert.ToInt32(_valueBox.Value);
        if (string.IsNullOrEmpty(name)) return;
        _result.Value = value;
        _result.Name = name;
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    public RankDTO GetResult()
    {
        return _result;
    }
}