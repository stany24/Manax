using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using ManaxClient.Event;
using ManaxClient.Models.Server.Data;
using ManaxClient.ViewModels;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.User;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models.Server.Sources;

public class UserSource
{
    public readonly SourceCache<User, long> Users = new(x => x.Id);
    private readonly Lock _usersLock = new();

    public UserSource()
    {
        NotificationReceiver.OnUserCreated += OnUserCreated;
        NotificationReceiver.OnUserDeleted += OnUserDeleted;
        WeakReferenceMessenger.Default.Register<LoggedInMessage>(this, (_, _) =>
        {
            if (MainWindowViewModel.Instance.PermissionManager.CanReadUsers)
                LoadUsers();
        });
    }

    private void LoadUsers()
    {
        Task.Run(async void () =>
        {
            try
            {
                Optional<List<long>> usersIdsResponse = await ManaxApiUserClient.GetUsersIdsAsync();
                if (usersIdsResponse.Failed)
                {
                    Logger.LogFailure(usersIdsResponse.Error);
                    WeakReferenceMessenger.Default.Send(new NotificationMessage(usersIdsResponse.Error));
                    return;
                }

                List<long> ids = usersIdsResponse.GetValue();
                foreach (long id in ids)
                {
                    Optional<UserDto> userResponse = await ManaxApiUserClient.GetUserAsync(id);
                    if (userResponse.Failed)
                    {
                        Logger.LogFailure(userResponse.Error);
                        WeakReferenceMessenger.Default.Send(new NotificationMessage(userResponse.Error));
                        continue;
                    }

                    UserDto dto = userResponse.GetValue();
                    lock (_usersLock)
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

    private void OnUserDeleted(long id)
    {
        lock (_usersLock)
        {
            Users.RemoveKey(id);
        }
    }

    private void OnUserCreated(UserDto user)
    {
        lock (_usersLock)
        {
            Users.AddOrUpdate(new User(user));
        }
    }
}