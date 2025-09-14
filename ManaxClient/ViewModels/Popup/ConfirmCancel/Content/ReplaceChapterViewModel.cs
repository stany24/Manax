using CommunityToolkit.Mvvm.ComponentModel;

namespace ManaxClient.ViewModels.Popup.ConfirmCancel.Content;

public partial class ReplaceChapterViewModel : ConfirmCancelContentViewModel
{
    [ObservableProperty] private string _folderPath;

    public ReplaceChapterViewModel(string folderPath)
    {
        _folderPath = folderPath;
        CanConfirm = true;
    }

    public string GetResult()
    {
        return FolderPath;
    }
}
