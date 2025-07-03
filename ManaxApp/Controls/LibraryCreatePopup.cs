using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using ManaxLibrary.DTOs.Library;

namespace ManaxApp.Controls;

public class LibraryCreatePopup : Popup
{
    private readonly Button _cancelButton;
    private readonly TextBox _nameBox;
    private readonly Button _okButton;
    private readonly TextBox _pathBox;
    private LibraryCreateDTO? _result;

    public LibraryCreatePopup()
    {
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
        _cancelButton.Click += CancelButton_Click;

        buttonGrid.Children.Add(_okButton);
        buttonGrid.Children.Add(_cancelButton);
        Grid.SetRow(buttonGrid, 3);
        Grid.SetColumn(buttonGrid, 0);
        Grid.SetColumnSpan(buttonGrid, 3);
        grid.Children.Add(buttonGrid);

        Content = grid;
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        string? name = _nameBox.Text?.Trim();
        string? path = _pathBox.Text?.Trim();
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(path)) return;
        _result = new LibraryCreateDTO { Name = name, Path = path };
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        _result = null;
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    public LibraryCreateDTO? GetResult()
    {
        return _result;
    }
}