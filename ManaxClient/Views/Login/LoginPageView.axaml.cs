using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ManaxClient.Views.Login;

// ReSharper disable once UnusedType.Global
public partial class LoginPageView : UserControl
{
    public LoginPageView()
    {
        InitializeComponent();
    }

    private void TbxUsername_OnLoaded(object? sender, RoutedEventArgs e)
    {
        TbxUsername.Focus();
    }
}