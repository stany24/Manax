using CommunityToolkit.Mvvm.ComponentModel;

namespace ManaxClient.ViewModels.Popup.ConfirmCancel;

public abstract partial class ConfirmCancelContentViewModel:ObservableObject
{
    [ObservableProperty] private bool _canConfirm;
    [ObservableProperty] private bool _canCancel = true;
}