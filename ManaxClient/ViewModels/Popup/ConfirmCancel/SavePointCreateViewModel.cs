using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.DTO.SavePoint;

namespace ManaxClient.ViewModels.Popup.ConfirmCancel;

public partial class SavePointCreateViewModel : ConfirmCancelContentViewModel
{
    [ObservableProperty] private string _path = string.Empty;

    public SavePointCreateViewModel()
    {
        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(Path))
            {
                CanConfirm = !string.IsNullOrWhiteSpace(Path);
            }
        };
    }

    public SavePointCreateDto GetResult()
    {
        return new SavePointCreateDto { Path = Path.Trim() };
    }
}
