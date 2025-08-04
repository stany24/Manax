using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using ManaxLibrary.DTOs.Serie;

namespace ManaxClient.Controls.Popups.Serie;

public class SerieUpdatePopup : ConfirmCancelPopup
{
    private TextBox _titleBox;
    private TextBox _descriptionBox;
    private ComboBox _statusComboBox;
    private SerieUpdateDTO? _result;
    private SerieDTO _serie;

    public SerieUpdatePopup(SerieDTO serie) : base()
    {
        _serie = serie;
    }

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
        _titleBox.Text = _serie.Title;

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
        _descriptionBox.Text = _serie.Description;
        
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
        _statusComboBox.SelectedItem = _serie.Status;

        return grid;
    }

    protected override void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        string? title = _titleBox.Text?.Trim();
        string? description = _descriptionBox.Text?.Trim();
        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description) || _statusComboBox.SelectedItem is not Status status) return;
        _result = new SerieUpdateDTO { Title = title, Description = description, Status = status};
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    public SerieUpdateDTO? GetResult()
    {
        return _result;
    }
}