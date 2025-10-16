using CommunityToolkit.Mvvm.ComponentModel;

namespace ManaxClient.ViewModels.Popup.ConfirmCancel.Content;

public abstract partial class ConfirmCancelContentViewModel : ObservableObject
{
    [ObservableProperty] private bool _canCancel = true;
    [ObservableProperty] private bool _canConfirm;
}