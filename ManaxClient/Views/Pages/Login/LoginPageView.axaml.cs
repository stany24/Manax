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

    public void TbxPasswordOnLoaded(object? sender, RoutedEventArgs e)
    {
        if (TbxUsername.Text == string.Empty)
        {
            TbxUsername.Focus();
            return;
        }

        TbxPassword.Focus();
    }
}