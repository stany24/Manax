using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using ManaxLibrary.DTOs.User;

namespace ManaxClient.Controls.Popups;

public class UserCreatePopup : Popup
{
    private readonly TextBox _usernameBox;
    private readonly TextBox _passwordBox;
    private readonly ComboBox _roleComboBox;
    private readonly Button _cancelButton;
    private readonly Button _okButton;
    private UserCreateDTO? _result;

    public UserCreatePopup(bool owner)
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

        TextBlock usernameLabel = new()
        {
            Text = "Username:",
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(usernameLabel, 0);
        Grid.SetColumn(usernameLabel, 0);
        grid.Children.Add(usernameLabel);

        _usernameBox = new TextBox();
        Grid.SetRow(_usernameBox, 0);
        Grid.SetColumn(_usernameBox, 2);
        grid.Children.Add(_usernameBox);

        TextBlock passwordLabel = new()
        {
            Text = "Password:",
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(passwordLabel, 1);
        Grid.SetColumn(passwordLabel, 0);
        grid.Children.Add(passwordLabel);

        _passwordBox = new TextBox();
        Grid.SetRow(_passwordBox, 1);
        Grid.SetColumn(_passwordBox, 2);
        grid.Children.Add(_passwordBox);
        
        TextBlock roleLabel = new()
        {
            Text = "Role:",
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(roleLabel, 2);
        Grid.SetColumn(roleLabel, 0);
        grid.Children.Add(roleLabel);

        _roleComboBox = new ComboBox();
        Grid.SetRow(_roleComboBox, 2);
        Grid.SetColumn(_roleComboBox, 2);
        grid.Children.Add(_roleComboBox);
        _roleComboBox.Items.Add(UserRole.User);
        _roleComboBox.SelectedItem = UserRole.User;
        if (owner)
        {
            _roleComboBox.Items.Add(UserRole.Admin);
        }

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
        
        Form.Content = grid;
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        string? username = _usernameBox.Text?.Trim();
        string? password = _passwordBox.Text?.Trim();
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || _roleComboBox.SelectedItem is not UserRole role) return;
        _result = new UserCreateDTO { Username = username, Password = password, Role = role};
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        _result = null;
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    public UserCreateDTO? GetResult()
    {
        return _result;
    }
}