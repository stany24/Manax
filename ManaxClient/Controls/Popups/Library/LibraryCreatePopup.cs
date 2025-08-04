using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using ManaxLibrary.DTOs.Library;

namespace ManaxClient.Controls.Popups.Library;

public class LibraryCreatePopup : ConfirmCancelPopup
{
    private TextBox _nameBox;
    private TextBox _pathBox;
    private LibraryCreateDTO? _result;

    protected override Grid GetFormGrid()
    {
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

        _nameBox = new TextBox();
        Grid.SetRow(_nameBox, 0);
        Grid.SetColumn(_nameBox, 2);
        grid.Children.Add(_nameBox);

        TextBlock pathLabel = new()
        {
            Text = "Path:",
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(pathLabel, 1);
        Grid.SetColumn(pathLabel, 0);
        grid.Children.Add(pathLabel);

        _pathBox = new TextBox();
        Grid.SetRow(_pathBox, 1);
        Grid.SetColumn(_pathBox, 2);
        grid.Children.Add(_pathBox);

        return grid;
    }

    protected override void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        string? name = _nameBox.Text?.Trim();
        string? path = _pathBox.Text?.Trim();
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(path)) return;
        _result = new LibraryCreateDTO { Name = name, Path = path };
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    public LibraryCreateDTO? GetResult()
    {
        return _result;
    }
}