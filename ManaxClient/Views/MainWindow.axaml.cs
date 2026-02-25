using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Messaging;
using ManaxClient.Event;

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
        PointerPointProperties properties = e.GetCurrentPoint(this).Properties;
        if (properties.IsXButton1Pressed)
        {
            WeakReferenceMessenger.Default.Send(new PreviousPageMessage());
            e.Handled = true;
        }
        else if (properties.IsXButton2Pressed)
        {
            WeakReferenceMessenger.Default.Send(new NextPageMessage());
            e.Handled = true;
        }
    }
}