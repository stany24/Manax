using CommunityToolkit.Mvvm.ComponentModel;

namespace ManaxClient.ViewModels.Popup.ConfirmCancel;

public partial class ConfirmCancelViewModel: PopupViewModel
{
    [ObservableProperty] private ConfirmCancelContentViewModel _content;
    private bool _canceled;

    public ConfirmCancelViewModel(ConfirmCancelContentViewModel content)
    {
        Content = content;
    }
    
    public void Confirm()
    {
        _canceled = false;
    }
    
    public void Cancel()
    {
        _canceled = true;
    }

    public bool Canceled()
    {
        return _canceled;
    }

    public override bool CloseAccepted()
    {
        if (!Content.CanCancel) return false;
        _canceled = true;
        return true;

    }
}