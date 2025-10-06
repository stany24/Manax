using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.User;
using ManaxLibrary.Logging;
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
    
    public static EventHandler<string>? ErrorEmitted { get; set; }

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
        Task.Run(async void () =>
        {
            try
            {
                Optional<List<long>> usersIdsResponse = await ManaxApiUserClient.GetUsersIdsAsync();
                if (usersIdsResponse.Failed)
                {
                    Logger.LogFailure(usersIdsResponse.Error);
                    ErrorEmitted?.Invoke(null, usersIdsResponse.Error);
                    return;
                }

                List<long> ids = usersIdsResponse.GetValue();
                foreach (long id in ids)
                {
                    Optional<UserDto> userResponse = await ManaxApiUserClient.GetUserAsync(id);
                    if (userResponse.Failed)
                    {
                    
                        Logger.LogFailure(userResponse.Error);
                        ErrorEmitted?.Invoke(null, userResponse.Error);
                        continue;
                    }

                    UserDto dto = userResponse.GetValue();
                    lock (UsersLock)
                    {
                        Users.AddOrUpdate(new User(dto));
                    }
                }
            }
            catch (Exception e)
            {
                const string error = "Failed to load users from server";
                Logger.LogError(error,e);
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