using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace ManaxClient.Controls.Popups;

public abstract class ConfirmCancelPopup : Popup
{
    private readonly Button _okButton;
    private readonly Button _cancelButton;

    protected ConfirmCancelPopup(string confirmText = "Confirm", string cancelText = "Cancel")
    {
        _okButton = new Button
        {
            HotKey = new KeyGesture(Key.Enter),
            Content = confirmText,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };
        _okButton.Click += OkButton_Click;

        _cancelButton = new Button
        {
            Content = cancelText,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };
        _cancelButton.Click += CancelButton_Click;

        Grid mainGrid = GetFormGrid();
        int buttonRow = mainGrid.RowDefinitions.Count;
        mainGrid.RowDefinitions.Add(new RowDefinition());
        Grid buttonGrid = CreateButtonGrid(buttonRow);
        mainGrid.Children.Add(buttonGrid);
        Form.Content = mainGrid;
    }

    protected abstract Grid GetFormGrid();

    private Grid CreateButtonGrid(int row, int columnSpan = 3)
    {
        Grid buttonGrid = new()
        {
            ColumnDefinitions = new ColumnDefinitions("*,10,*")
        };
        Grid.SetColumn(_okButton, 2);
        Grid.SetColumn(_cancelButton, 0);
        buttonGrid.Children.Add(_okButton);
        buttonGrid.Children.Add(_cancelButton);
        Grid.SetRow(buttonGrid, row);
        Grid.SetColumn(buttonGrid, 0);
        Grid.SetColumnSpan(buttonGrid, columnSpan);
        return buttonGrid;
    }

    protected abstract void OkButton_Click(object? sender, RoutedEventArgs e);

    protected virtual void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}

