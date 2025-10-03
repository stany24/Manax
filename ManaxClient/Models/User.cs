using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.User;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models;

public partial class User:ObservableObject
{
    public static readonly SourceCache<User, long> Users = new (x => x.Id);
    private static readonly object UsersLock = new();
    
    [ObservableProperty] private long _id;
    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private UserRole _role;
    [ObservableProperty] private DateTime _lastLogin;

    static User()
    {
        ServerNotification.OnUserCreated += OnUserCreated;
        ServerNotification.OnUserDeleted += OnUserDeleted;
        LoadUsers();
    }
    
    public User(UserDto dto)
    {
        FromUserDto(dto);
        ServerNotification.OnUserUpdated += OnUserUpdated;
    }

    ~User()
    {
        ServerNotification.OnUserUpdated -= OnUserUpdated;
    }

    private static void LoadUsers()
    {
        Task.Run(async () =>
        {
            Optional<List<long>> usersIdsResponse = await ManaxApiUserClient.GetUsersIdsAsync();
            if (usersIdsResponse.Failed)
            {
                return;
            }

            List<long> ids = usersIdsResponse.GetValue();
            foreach (long id in ids)
            {
                Optional<UserDto> userResponse = await ManaxApiUserClient.GetUserAsync(id);
                if (userResponse.Failed)
                {
                    continue;
                }

                UserDto dto = userResponse.GetValue();
                lock (UsersLock)
                {
                    Users.AddOrUpdate(new User(dto));
                }
            }
        });
    }
    
    private static void OnUserDeleted(long id)
    {
        lock (UsersLock)
        {
            Users.RemoveKey(id);   
        }
    }

    private static void OnUserCreated(UserDto user)
    {
        lock (UsersLock)
        {
            Users.AddOrUpdate(new User(user));
        }
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