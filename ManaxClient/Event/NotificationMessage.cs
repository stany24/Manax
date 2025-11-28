using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ManaxClient.Event;

public class NotificationMessage(string user) : ValueChangedMessage<string>(user);