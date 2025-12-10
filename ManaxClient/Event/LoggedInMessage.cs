using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ManaxClient.Event;

public class LoggedInMessage(string token) : ValueChangedMessage<string>(token);