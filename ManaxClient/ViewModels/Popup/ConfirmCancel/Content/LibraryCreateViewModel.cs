using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.DTO.Library;

namespace ManaxClient.ViewModels.Popup.ConfirmCancel.Content;

public partial class LibraryCreateViewModel : ConfirmCancelContentViewModel
{
    [ObservableProperty] private string _name = string.Empty;

    public LibraryCreateViewModel()
    {
        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(Name))
            {
                CanConfirm = !string.IsNullOrWhiteSpace(Name);
            }
        };
    }

    public LibraryCreateDto GetResult()
    {
        return new LibraryCreateDto { Name = Name.Trim() };
    }
}
