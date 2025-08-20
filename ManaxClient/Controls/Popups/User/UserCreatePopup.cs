using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using ManaxLibrary.DTO.User;

namespace ManaxClient.Controls.Popups.User;

public class UserCreatePopup(bool owner) : ConfirmCancelPopup("Créer")
{
    private TextBox _passwordBox = null!;
    private UserCreateDto? _result;
    private ComboBox _roleComboBox = null!;
    private TextBox _usernameBox = null!;

    protected override Grid GetFormGrid()
    {
        Grid grid = new()
        {
            RowSpacing = 16,
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto,Auto,Auto"),
            ColumnDefinitions = new ColumnDefinitions("*")
        };

        StackPanel headerPanel = new()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Margin = new Thickness(0, 0, 0, 8)
        };

        TextBlock icon = new()
        {
            Text = "👤",
            FontSize = 24,
            VerticalAlignment = VerticalAlignment.Center
        };

        StackPanel titleStack = new()
        {
            Orientation = Orientation.Vertical,
            Spacing = 4
        };

        TextBlock title = new()
        {
            Text = "Nouvel utilisateur",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#212529"))
        };

        TextBlock subtitle = new()
        {
            Text = "Créez un nouveau compte utilisateur pour accéder à Manax",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#6C757D"))
        };

        titleStack.Children.Add(title);
        titleStack.Children.Add(subtitle);
        headerPanel.Children.Add(icon);
        headerPanel.Children.Add(titleStack);

        Grid.SetRow(headerPanel, 0);
        grid.Children.Add(headerPanel);

        StackPanel usernamePanel = new()
        {
            Spacing = 8
        };

        TextBlock usernameLabel = new()
        {
            Text = "Nom d'utilisateur",
            FontWeight = FontWeight.Medium,
            Foreground = new SolidColorBrush(Color.Parse("#495057"))
        };

        _usernameBox = new TextBox
        {
            Watermark = "Entrez le nom d'utilisateur...",
            Padding = new Thickness(12),
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.Parse("#DEE2E6")),
            FontSize = 14
        };

        usernamePanel.Children.Add(usernameLabel);
        usernamePanel.Children.Add(_usernameBox);
        Grid.SetRow(usernamePanel, 1);
        grid.Children.Add(usernamePanel);

        StackPanel passwordPanel = new()
        {
            Spacing = 8
        };

        TextBlock passwordLabel = new()
        {
            Text = "Mot de passe",
            FontWeight = FontWeight.Medium,
            Foreground = new SolidColorBrush(Color.Parse("#495057"))
        };

        _passwordBox = new TextBox
        {
            Watermark = "Entrez le mot de passe...",
            PasswordChar = '•',
            Padding = new Thickness(12),
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.Parse("#DEE2E6")),
            FontSize = 14
        };

        passwordPanel.Children.Add(passwordLabel);
        passwordPanel.Children.Add(_passwordBox);
        Grid.SetRow(passwordPanel, 2);
        grid.Children.Add(passwordPanel);

        StackPanel rolePanel = new()
        {
            Spacing = 8
        };

        TextBlock roleLabel = new()
        {
            Text = "Rôle utilisateur",
            FontWeight = FontWeight.Medium,
            Foreground = new SolidColorBrush(Color.Parse("#495057"))
        };

        _roleComboBox = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Padding = new Thickness(12),
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.Parse("#DEE2E6"))
        };

        _roleComboBox.Items.Add(new ComboBoxItem
        {
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    new TextBlock { Text = "👤", FontSize = 14 },
                    new TextBlock { Text = "Utilisateur standard", FontWeight = FontWeight.Medium }
                }
            },
            Tag = UserRole.User
        });

        if (owner)
            _roleComboBox.Items.Add(new ComboBoxItem
            {
                Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 8,
                    Children =
                    {
                        new TextBlock { Text = "👑", FontSize = 14 },
                        new TextBlock { Text = "Administrateur", FontWeight = FontWeight.Medium }
                    }
                },
                Tag = UserRole.Admin
            });

        _roleComboBox.SelectedIndex = 0;

        rolePanel.Children.Add(roleLabel);
        rolePanel.Children.Add(_roleComboBox);
        Grid.SetRow(rolePanel, 3);
        grid.Children.Add(rolePanel);

        Border helpBorder = new()
        {
            Background = new SolidColorBrush(Color.Parse("#FFF3CD")),
            BorderBrush = new SolidColorBrush(Color.Parse("#FFC107")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12)
        };

        StackPanel helpPanel = new()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8
        };

        TextBlock helpIcon = new()
        {
            Text = "⚠️",
            FontSize = 14,
            VerticalAlignment = VerticalAlignment.Top
        };

        TextBlock helpText = new()
        {
            Text =
                "Les administrateurs ont accès à toutes les fonctionnalités de gestion. Choisissez ce rôle avec précaution.",
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#495057"))
        };

        helpPanel.Children.Add(helpIcon);
        helpPanel.Children.Add(helpText);
        helpBorder.Child = helpPanel;

        Grid.SetRow(helpBorder, 4);
        grid.Children.Add(helpBorder);

        ApplyInputStyles(_usernameBox);
        ApplyInputStyles(_passwordBox);
        ApplyInputStyles(_roleComboBox);

        return grid;
    }

    private static void ApplyInputStyles(Control control)
    {
        control.PointerEntered += (s, _) =>
        {
            if (s is Control c) SetBorderBrush(c, new SolidColorBrush(Color.Parse("#007ACC")));
        };

        control.PointerExited += (s, _) =>
        {
            if (s is Control { IsFocused: false } c)
                SetBorderBrush(c, new SolidColorBrush(Color.Parse("#DEE2E6")));
        };

        control.GotFocus += (s, _) =>
        {
            if (s is Control c) SetBorderBrush(c, new SolidColorBrush(Color.Parse("#007ACC")));
        };

        control.LostFocus += (s, _) =>
        {
            if (s is Control c) SetBorderBrush(c, new SolidColorBrush(Color.Parse("#DEE2E6")));
        };
    }

    private static void SetBorderBrush(Control control, IBrush brush)
    {
        switch (control)
        {
            case TextBox tb:
                tb.BorderBrush = brush;
                break;
            case ComboBox cb:
                cb.BorderBrush = brush;
                break;
        }
    }

    protected override void OkButtonClicked(object? sender, RoutedEventArgs e)
    {
        string? username = _usernameBox.Text?.Trim();
        string? password = _passwordBox.Text?.Trim();

        if (string.IsNullOrEmpty(username) ||
            string.IsNullOrEmpty(password) ||
            _roleComboBox.SelectedItem is not ComboBoxItem { Tag: UserRole role })
            return;

        _result = new UserCreateDto { Username = username, Password = password, Role = role };
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    public UserCreateDto? GetResult()
    {
        return _result;
    }
}