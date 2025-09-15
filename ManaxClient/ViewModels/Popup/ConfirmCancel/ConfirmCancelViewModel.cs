using System;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.ViewModels.Popup.ConfirmCancel.Content;

namespace ManaxClient.ViewModels.Popup.ConfirmCancel;

public partial class ConfirmCancelViewModel : PopupViewModel
{
    private bool _canceled;
    [ObservableProperty] private ConfirmCancelContentViewModel _content;

    public ConfirmCancelViewModel(ConfirmCancelContentViewModel content)
    {
        Content = content;
    }

    public void Confirm()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
        _canceled = false;
    }

    public void Cancel()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
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