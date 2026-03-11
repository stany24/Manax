namespace ManaxLibrary.Notifications;

public static class NotificationInformation
{
    public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan KeepAliveInterval = TimeSpan.FromSeconds(Timeout.TotalSeconds/2-1);
}