using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Media;
using ManaxClient.Event;
using ManaxClient.Localization.Localizer;

namespace ManaxClient.Controls.Previews;

public class NotificationPreview : Button
{
    public static readonly AttachedProperty<Notification?> NotificationProperty =
        AvaloniaProperty.RegisterAttached<NotificationPreview, NotificationPreview, Notification?>(
            "Notification", null, false, BindingMode.OneTime);

    public NotificationPreview()
    {
        Click += (_, _) =>
        {
            Notification?.Remove();
        };
        Localizer.LanguageChanged += (_, _) =>
            Content = string.Format(Localizer.Get(Notification.LocalizationKey), Notification.Args);
        Bind(ContentProperty, new Binding(nameof(Notification))
        {
            Source = this,
            Converter = new FuncValueConverter<Notification, string>(notification =>
                notification != null ? string.Format(Localizer.Get(notification.LocalizationKey), notification.Args): "")
        });
    }

    public Notification? Notification
    {
        get => GetNotification(this);
        set => SetNotification(this, value);
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        RenderTransform = new ScaleTransform(1.05, 1.05);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        RenderTransform = new ScaleTransform(1.0, 1.0);
    }

    private static void SetNotification(AvaloniaObject element, Notification? serieValue)
    {
        element.SetValue(NotificationProperty, serieValue);
    }

    private static Notification? GetNotification(AvaloniaObject element)
    {
        return element.GetValue(NotificationProperty);
    }
}