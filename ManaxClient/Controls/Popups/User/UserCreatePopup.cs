using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using ManaxLibrary.DTOs.User;

namespace ManaxClient.Controls.Popups.User;

public class UserCreatePopup(bool owner) : ConfirmCancelPopup
{
    private TextBox _usernameBox = null!;
    private TextBox _passwordBox = null!;
    private ComboBox _roleComboBox = null!;
    private UserCreateDTO? _result;

    protected override Grid GetFormGrid()
    {
        Grid grid = new()
        {
            Margin = new Thickness(10),
            RowSpacing = 5,
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto"),
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

        return grid;
    }

    protected override void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        string? username = _usernameBox.Text?.Trim();
        string? password = _passwordBox.Text?.Trim();
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || _roleComboBox.SelectedItem is not UserRole role) return;
        _result = new UserCreateDTO { Username = username, Password = password, Role = role};
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    public UserCreateDTO? GetResult()
    {
        return _result;
    }
}