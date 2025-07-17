using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace ManaxClient.Controls.Popups;

public abstract class Popup : UserControl
{
    public EventHandler? Closed;
    public EventHandler? CloseRequested;

    protected Popup()
    {
        Background = Brushes.White;
        CornerRadius = new CornerRadius(5);
    }

    public void Close()
    {
        Closed?.Invoke(this, EventArgs.Empty);
    }
}