using CommunityToolkit.Mvvm.Messaging.Messages;
using ManaxClient.Controls.Popups;

namespace ManaxClient.Event;

public class PopupChangeMessage(Popup popup) : ValueChangedMessage<Popup>(popup);