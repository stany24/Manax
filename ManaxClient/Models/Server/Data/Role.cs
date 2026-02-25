using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.DTO.Role;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models.Server.Data;

public partial class Role : ObservableObject
{
    [ObservableProperty] private long _id;
    [ObservableProperty] private string _name = string.Empty;

    public Role(RoleDto dto)
    {
        NotificationReceiver.OnRoleUpdated += OnRoleUpdated;
        FromDto(dto);
    }

    ~Role()
    {
        NotificationReceiver.OnRoleUpdated -= OnRoleUpdated;
    }

    private void FromDto(RoleDto dto)
    {
        Id = dto.Id;
        Name = dto.Name;
    }

    private void OnRoleUpdated(RoleDto dto)
    {
        if (Id != dto.Id) return;
        FromDto(dto);
    }
}