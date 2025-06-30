using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace ManaxApp.Controls;

public abstract class Popup: UserControl
{
    public EventHandler? CloseRequested;
    public EventHandler? Closed;

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