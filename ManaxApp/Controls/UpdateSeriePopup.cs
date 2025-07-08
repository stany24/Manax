using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using ManaxLibrary.DTOs.Serie;

namespace ManaxApp.Controls;

public class SerieUpdatePopup : Popup
{
    private readonly Button _cancelButton;
    private readonly TextBox _titleBox;
    private readonly Button _okButton;
    private readonly TextBox _descriptionBox;
    private readonly ComboBox _statusComboBox;
    private SerieUpdateDTO? _result;

    public SerieUpdatePopup(SerieDTO serie)
    {
        MinWidth = 200;
        MaxWidth = 500;
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Center;

        Grid grid = new()
        {
            Margin = new Thickness(10),
            RowSpacing = 5,
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto,Auto"),
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

        _titleBox = new TextBox();
        Grid.SetRow(_titleBox, 0);
        Grid.SetColumn(_titleBox, 2);
        grid.Children.Add(_titleBox);
        _titleBox.Text = serie.Title;

        TextBlock descriptionLabel = new()
        {
            Text = "Description:",
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(descriptionLabel, 1);
        Grid.SetColumn(descriptionLabel, 0);
        grid.Children.Add(descriptionLabel);

        _descriptionBox = new TextBox();
        Grid.SetRow(_descriptionBox, 1);
        Grid.SetColumn(_descriptionBox, 2);
        grid.Children.Add(_descriptionBox);
        _descriptionBox.Text = serie.Description;
        
        TextBlock statusLabel = new()
        {
            Text = "Status:",
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(statusLabel, 2);
        Grid.SetColumn(statusLabel, 0);
        grid.Children.Add(statusLabel);

        _statusComboBox = new ComboBox();
        Grid.SetRow(_statusComboBox, 2);
        Grid.SetColumn(_statusComboBox, 2);
        grid.Children.Add(_statusComboBox);
        foreach (object value in Enum.GetValues(typeof(Status))) { _statusComboBox.Items.Add(value); }
        _statusComboBox.SelectedItem = serie.Status;

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
        Grid.SetRow(buttonGrid, 4);
        Grid.SetColumn(buttonGrid, 0);
        Grid.SetColumnSpan(buttonGrid, 3);
        grid.Children.Add(buttonGrid);

        Content = grid;
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        string? title = _titleBox.Text?.Trim();
        string? description = _descriptionBox.Text?.Trim();
        Status? status = _statusComboBox.SelectedItem as Status?;
        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description) || status == null) return;
        _result = new SerieUpdateDTO { Title = title, Description = description, Status = (Status)status};
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        _result = null;
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    public SerieUpdateDTO? GetResult()
    {
        return _result;
    }
}