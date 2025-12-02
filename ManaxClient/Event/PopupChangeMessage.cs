using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ManaxClient.Event;

public class PopupChangeMessage(Controls.Popups.Popup popup) : ValueChangedMessage<Controls.Popups.Popup>(popup);