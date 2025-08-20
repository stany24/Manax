using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

namespace ManaxClient.Controls.Popups;

public abstract class ConfirmCancelPopup : Popup
{
    private readonly Button _cancelButton;
    private readonly Button _okButton;

    protected ConfirmCancelPopup(string confirmText = "Confirmer", string cancelText = "Annuler")
    {
        _okButton = new Button
        {
            HotKey = new KeyGesture(Key.Enter),
            Content = confirmText,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Background = new SolidColorBrush(Color.Parse("#007ACC")),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16, 12),
            FontWeight = FontWeight.Medium,
            MinWidth = 100
        };
        _okButton.Click += OkButtonClicked;

        _cancelButton = new Button
        {
            Content = cancelText,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Background = new SolidColorBrush(Color.Parse("#E9ECEF")),
            Foreground = new SolidColorBrush(Color.Parse("#495057")),
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16, 12),
            FontWeight = FontWeight.Medium,
            MinWidth = 100
        };
        _cancelButton.Click += CancelButtonClicked;

        _okButton.PointerEntered += (s, _) =>
        {
            if (s is Button btn) btn.Background = new SolidColorBrush(Color.Parse("#005A9E"));
        };
        _okButton.PointerExited += (s, _) =>
        {
            if (s is Button btn) btn.Background = new SolidColorBrush(Color.Parse("#007ACC"));
        };

        _cancelButton.PointerEntered += (s, _) =>
        {
            if (s is Button btn) btn.Background = new SolidColorBrush(Color.Parse("#DEE2E6"));
        };
        _cancelButton.PointerExited += (s, _) =>
        {
            if (s is Button btn) btn.Background = new SolidColorBrush(Color.Parse("#E9ECEF"));
        };

        Grid mainGrid = GetFormGrid();
        int buttonRow = mainGrid.RowDefinitions.Count;
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

        Border separator = new()
        {
            Height = 1,
            Background = new SolidColorBrush(Color.Parse("#E9ECEF")),
            Margin = new Thickness(0, 16, 0, 16)
        };
        Grid.SetRow(separator, buttonRow);
        Grid.SetColumnSpan(separator, 3);
        mainGrid.Children.Add(separator);

        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
        Grid buttonGrid = CreateButtonGrid(buttonRow + 1);
        mainGrid.Children.Add(buttonGrid);

        Form.Content = mainGrid;
        Form.Padding = new Thickness(24);
        Form.MinWidth = 400;
    }

    public bool Canceled { get; internal set; }

    protected abstract Grid GetFormGrid();

    private Grid CreateButtonGrid(int row, int columnSpan = 3)
    {
        Grid buttonGrid = new()
        {
            ColumnDefinitions = new ColumnDefinitions("*,16,*"),
            Margin = new Thickness(0)
        };

        Grid.SetColumn(_cancelButton, 0);
        Grid.SetColumn(_okButton, 2);

        buttonGrid.Children.Add(_cancelButton);
        buttonGrid.Children.Add(_okButton);

        Grid.SetRow(buttonGrid, row);
        Grid.SetColumn(buttonGrid, 0);
        Grid.SetColumnSpan(buttonGrid, columnSpan);

        return buttonGrid;
    }

    protected abstract void OkButtonClicked(object? sender, RoutedEventArgs e);

    protected virtual void CancelButtonClicked(object? sender, RoutedEventArgs e)
    {
        Canceled = true;
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}