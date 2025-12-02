using CommunityToolkit.Mvvm.Messaging.Messages;
using ManaxClient.ViewModels.Pages;

namespace ManaxClient.Event;

public class PageChangeMessage(PageViewModel page) : ValueChangedMessage<PageViewModel>(page);