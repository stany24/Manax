using System;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.DTO.User;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models.Server.Data;

public partial class User : ObservableObject
{
    [ObservableProperty] private long _id;
    [ObservableProperty] private DateTime _lastLogin;
    [ObservableProperty] private UserRole _role;
    [ObservableProperty] private string _username = string.Empty;

    public User(UserDto dto)
    {
        FromUserDto(dto);
        NotificationReceiver.OnUserUpdated += OnUserUpdated;
    }

    ~User()
    {
        NotificationReceiver.OnUserUpdated -= OnUserUpdated;
    }

    private void OnUserUpdated(UserDto dto)
    {
        if (dto.Id != Id) return;
        FromUserDto(dto);
    }

    private void FromUserDto(UserDto dto)
    {
        Id = dto.Id;
        Username = dto.Username;
        Role = dto.Role;
        LastLogin = dto.LastLogin;
    }
}