using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Input;

namespace ManaxClient.Controls.Popups;

public abstract class Popup : Panel
{
    public EventHandler? Closed;
    public EventHandler? CloseRequested;
    private protected readonly UserControl Form = new()
    {
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        Background = Brushes.White,
        CornerRadius = new CornerRadius(5)
    };
    protected readonly Canvas Canvas;
    protected readonly Rectangle BackShadow;

    protected Popup()
    {
        Background = Brushes.Transparent;
        
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Stretch;
        
        Canvas = new Canvas
        {
            Background = Brushes.Transparent,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        
        BackShadow = new Rectangle
        {
            Fill = Brushes.Black,
            Opacity = 0.7,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        
        BackShadow.PointerPressed += BackShadowPointerPressed;
        BackShadow.Tapped += BackShadowTapped;
        
        Canvas.Children.Add(BackShadow);
        LogicalChildren.Add(Canvas);
        VisualChildren.Add(Canvas);
        
        LogicalChildren.Add(Form);
        VisualChildren.Add(Form);
        
        AttachedToVisualTree += Popup_AttachedToVisualTree;
    }
    
    private void Popup_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (VisualRoot is not Window window) return;
        UpdateBackgroundSize(window.Bounds.Width, window.Bounds.Height);
        window.PropertyChanged += (_, args) =>
        {
            if (args.Property == BoundsProperty || args.Property == WidthProperty || args.Property == HeightProperty)
            {
                UpdateBackgroundSize(window.Bounds.Width, window.Bounds.Height);
            }
        };
    }
    
    private void UpdateBackgroundSize(double width, double height)
    {
        if (width <= 0 || height <= 0) return;
        BackShadow.Width = width;
        BackShadow.Height = height;
        Canvas.Width = width;
        Canvas.Height = height;
    }
    
    private void BackShadowPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
        e.Handled = true;
    }
    
    private void BackShadowTapped(object? sender, TappedEventArgs e)
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
        e.Handled = true;
    }

    public void Close()
    {
        Closed?.Invoke(this, EventArgs.Empty);
    }
}