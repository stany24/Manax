using CommunityToolkit.Mvvm.ComponentModel;

namespace ManaxClient.ViewModels.Popup.ConfirmCancel.Content;

public abstract partial class ConfirmCancelContentViewModel:ViewModelBase
{
    [ObservableProperty] private bool _canConfirm;
    [ObservableProperty] private bool _canCancel = true;
}