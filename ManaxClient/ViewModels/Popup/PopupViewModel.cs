using System;

namespace ManaxClient.ViewModels.Popup;

public abstract class PopupViewModel : LocalizedViewModel
{
    public EventHandler? CloseRequested { get; set; }
    public abstract bool CloseAccepted();
}