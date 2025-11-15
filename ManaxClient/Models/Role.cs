using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.DTO.Role;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models;

public partial class Role : ObservableObject
{
    [ObservableProperty] private long _id;
    [ObservableProperty] private string _name = string.Empty;

    public Role(RoleDto dto)
    {
        ServerNotification.OnRoleUpdated += OnRoleUpdated;
        FromDto(dto);
    }

    ~Role()
    {
        ServerNotification.OnRoleUpdated -= OnRoleUpdated;
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