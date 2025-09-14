using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.DTO.User;

namespace ManaxClient.ViewModels.Popup.ConfirmCancel.Content;

public partial class UserCreateViewModel : ConfirmCancelContentViewModel
{
    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private UserRoleItem? _selectedRole;

    public ObservableCollection<UserRoleItem> Roles { get; } = [];

    public UserCreateViewModel(bool includeAdmin = false)
    {
        Roles.Add(new UserRoleItem { Role = UserRole.User, DisplayName = "👤 Utilisateur standard" });
        
        if (includeAdmin)
            Roles.Add(new UserRoleItem { Role = UserRole.Admin, DisplayName = "👑 Administrateur" });

        SelectedRole = Roles[0];

        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is nameof(Username) or nameof(Password))
            {
                CanConfirm = !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
            }
        };
    }

    public UserCreateDto GetResult()
    {
        return new UserCreateDto 
        { 
            Username = Username.Trim(), 
            Password = Password.Trim(), 
            Role = SelectedRole?.Role ?? UserRole.User
        };
    }
}

public class UserRoleItem
{
    public UserRole Role { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}
