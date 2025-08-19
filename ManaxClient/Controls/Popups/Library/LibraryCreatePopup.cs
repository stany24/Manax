using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using ManaxLibrary.DTO.Library;

namespace ManaxClient.Controls.Popups.Library;

public class LibraryCreatePopup : ConfirmCancelPopup
{
    private TextBox _nameBox = null!;
    private LibraryCreateDto? _result;

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

        _nameBox = new TextBox();
        Grid.SetRow(_nameBox, 0);
        Grid.SetColumn(_nameBox, 2);
        grid.Children.Add(_nameBox);

        return grid;
    }

    protected override void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        string? name = _nameBox.Text?.Trim();
        if (string.IsNullOrEmpty(name)) return;
        _result = new LibraryCreateDto { Name = name};
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    public LibraryCreateDto? GetResult()
    {
        return _result;
    }
}