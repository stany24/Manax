using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using ManaxLibrary.DTO.Serie;

namespace ManaxClient.Controls.Popups.Serie;

public class SerieUpdatePopup(SerieDto serie) : ConfirmCancelPopup
{
    private TextBox _titleBox = null!;
    private TextBox _descriptionBox = null!;
    private ComboBox _statusComboBox = null!;
    private SerieUpdateDto? _result;

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

        return grid;
    }

    protected override void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        string? title = _titleBox.Text?.Trim();
        string? description = _descriptionBox.Text?.Trim();
        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description) || _statusComboBox.SelectedItem is not Status status) return;
        _result = new SerieUpdateDto { Title = title, Description = description, Status = status};
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    public SerieUpdateDto? GetResult()
    {
        return _result;
    }
}