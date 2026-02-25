using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.DTO.Role;

namespace ManaxClient.ViewModels.Popup.ConfirmCancel.Content;

public partial class RoleEditViewModel : ConfirmCancelContentViewModel
{
    private readonly RoleUpdateDto _originalRole;
    [ObservableProperty] private string _name;

    public RoleEditViewModel(RoleUpdateDto role)
    {
        _originalRole = role;
        _name = role.Name;
        CanConfirm = true;

        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(Name)) CanConfirm = !string.IsNullOrWhiteSpace(Name);
        };
    }

    public RoleUpdateDto GetResult()
    {
        return new RoleUpdateDto
        {
            Name = Name.Trim()
        };
    }
}