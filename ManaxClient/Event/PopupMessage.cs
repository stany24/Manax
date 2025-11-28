using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ManaxClient.Event;

public class PopupMessage(Controls.Popups.Popup user) : ValueChangedMessage<Controls.Popups.Popup>(user);