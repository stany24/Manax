using Avalonia.Controls;
using Avalonia.Input;
using ManaxClient.ViewModels;

namespace ManaxClient.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        PointerPressed += MainWindow_PointerPressed;
    }

    private void MainWindow_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
            return;

        PointerPointProperties properties = e.GetCurrentPoint(this).Properties;
        if (properties.IsXButton1Pressed)
        {
            vm.GoBack();
            e.Handled = true;
        }
        else if (properties.IsXButton2Pressed)
        {
            vm.GoForward();
            e.Handled = true;
        }
    }
}