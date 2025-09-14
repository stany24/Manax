using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ManaxClient.ViewModels.Popup;

public abstract class PopupViewModel:ViewModelBase
{
    public EventHandler? CloseRequested;
    public abstract bool CloseAccepted();
}