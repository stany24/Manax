using System;

namespace ManaxClient.ViewModels.Popup;

public abstract class PopupViewModel : LocalizedObject
{
    public EventHandler? CloseRequested { get; set; }
    public abstract bool CloseAccepted();
}