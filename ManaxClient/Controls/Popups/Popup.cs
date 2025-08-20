using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

namespace ManaxClient.Controls.Popups;

public abstract class Popup : Panel
{
    private readonly Rectangle _backShadow;
    private readonly Canvas _canvas;

    private protected readonly UserControl Form = new()
    {
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        Background = Brushes.White,
        CornerRadius = new CornerRadius(12),
        Margin = new Thickness(20)
    };

    public EventHandler? Closed;
    public EventHandler? CloseRequested;

    protected Popup()
    {
        Background = Brushes.Transparent;
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Stretch;

        _canvas = new Canvas
        {
            Background = Brushes.Transparent,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        _backShadow = new Rectangle
        {
            Fill = new SolidColorBrush(Color.Parse("#000000")),
            Opacity = 0.5,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        _backShadow.PointerPressed += BackShadowPointerPressed;
        _backShadow.Tapped += BackShadowTapped;

        _canvas.Children.Add(_backShadow);
        LogicalChildren.Add(_canvas);
        VisualChildren.Add(_canvas);

        LogicalChildren.Add(Form);
        VisualChildren.Add(Form);

        AttachedToVisualTree += Popup_AttachedToVisualTree;

        Form.Transitions =
        [
            new TransformOperationsTransition
            {
                Property = RenderTransformProperty,
                Duration = TimeSpan.FromMilliseconds(200)
            },

            new DoubleTransition
            {
                Property = OpacityProperty,
                Duration = TimeSpan.FromMilliseconds(200)
            }
        ];

        _canvas.Transitions =
        [
            new DoubleTransition
            {
                Property = OpacityProperty,
                Duration = TimeSpan.FromMilliseconds(150)
            }
        ];

        Form.RenderTransform = new ScaleTransform(0.9, 0.9);
        Form.Opacity = 0;
        _canvas.Opacity = 0;

        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        _canvas.Opacity = 1;
        Form.Opacity = 1;
        Form.RenderTransform = new ScaleTransform(1.0, 1.0);
    }

    private void Popup_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (VisualRoot is not Window window) return;
        UpdateBackgroundSize(window.Bounds.Width, window.Bounds.Height);
        window.PropertyChanged += (_, args) =>
        {
            if (args.Property == BoundsProperty || args.Property == WidthProperty || args.Property == HeightProperty)
                UpdateBackgroundSize(window.Bounds.Width, window.Bounds.Height);
        };
    }

    private void UpdateBackgroundSize(double width, double height)
    {
        if (width <= 0 || height <= 0) return;
        _backShadow.Width = width;
        _backShadow.Height = height;
        _canvas.Width = width;
        _canvas.Height = height;
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

    public async void Close()
    {
        Form.Opacity = 0;
        Form.RenderTransform = new ScaleTransform(0.9, 0.9);
        _canvas.Opacity = 0;

        await Task.Delay(200);
        Closed?.Invoke(this, EventArgs.Empty);
    }
}