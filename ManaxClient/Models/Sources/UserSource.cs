using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DynamicData;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.User;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models.Sources;

public static class UserSource
{
    public static readonly SourceCache<User, long> Users = new(x => x.Id);
    private static readonly object UsersLock = new();

    static UserSource()
    {
        ServerNotification.OnUserCreated += OnUserCreated;
        ServerNotification.OnUserDeleted += OnUserDeleted;
        LoadUsers();
    }

    public static EventHandler<string>? ErrorEmitted { get; set; }

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
                Logger.LogError(error, e);
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
}