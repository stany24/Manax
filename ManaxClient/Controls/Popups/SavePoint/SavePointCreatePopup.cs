using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using ManaxLibrary.DTO.SavePoint;

namespace ManaxClient.Controls.Popups.SavePoint;

public class SavePointCreatePopup : ConfirmCancelPopup
{
    private TextBox _pathBox = null!;
    private SavePointCreateDto? _result;

    protected override Grid GetFormGrid()
    {
        Grid grid = new()
        {
            Margin = new Thickness(10),
            RowSpacing = 5,
            RowDefinitions = new RowDefinitions("Auto,Auto"),
            ColumnDefinitions = new ColumnDefinitions("Auto,10,*")
        };

        TextBlock pathLabel = new()
        {
            Text = "Path:",
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(pathLabel, 0);
        Grid.SetColumn(pathLabel, 0);
        grid.Children.Add(pathLabel);

        _pathBox = new TextBox();
        Grid.SetRow(_pathBox, 0);
        Grid.SetColumn(_pathBox, 2);
        grid.Children.Add(_pathBox);

        return grid;
    }

    protected override void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        string? path = _pathBox.Text?.Trim();
        if (string.IsNullOrEmpty(path)) return;
        _result = new SavePointCreateDto {Path = path };
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    public SavePointCreateDto? GetResult()
    {
        return _result;
    }
}