using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ManaxClient.Views.Pages.Login;

// ReSharper disable once UnusedType.Global
public partial class LoginPageView : UserControl
{
    public LoginPageView()
    {
        InitializeComponent();
    }

    public void TbxUsernameOnLoaded(object? sender, RoutedEventArgs e)
    {
        TbxUsername.Focus();
    }
}