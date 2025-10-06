using System;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.DTO.User;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models;

public partial class User:ObservableObject
{
    [ObservableProperty] private long _id;
    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private UserRole _role;
    [ObservableProperty] private DateTime _lastLogin;
    
    public User(UserDto dto)
    {
        FromUserDto(dto);
        ServerNotification.OnUserUpdated += OnUserUpdated;
    }

    ~User()
    {
        ServerNotification.OnUserUpdated -= OnUserUpdated;
    }

    private void OnUserUpdated(UserDto dto)
    {
         if(dto.Id != Id){return;}
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