using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ManaxClient.Event;

public class NotificationMessage(Notification notification) : ValueChangedMessage<Notification>(notification);