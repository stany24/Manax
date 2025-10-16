using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ManaxClient.ViewModels.Popup;

public abstract class PopupViewModel : ObservableObject
{
    public EventHandler? CloseRequested { get; set; }
    public abstract bool CloseAccepted();
}