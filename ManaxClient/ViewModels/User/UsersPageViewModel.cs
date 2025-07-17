using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTOs.User;
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels.User;

public partial class UsersPageViewModel : PageViewModel
{
    [ObservableProperty] private ObservableCollection<UserDTO> _users = [];

    public UsersPageViewModel()
    {
        Task.Run(async () =>
        {
            List<long>? ids = await ManaxApiUserClient.GetUsersIdsAsync();
            if (ids == null) return;
            foreach (long id in ids)
            {
                UserDTO? userAsync = await ManaxApiUserClient.GetUserAsync(id);
                if (userAsync == null) continue;
                Dispatcher.UIThread.Post(() => Users.Add(userAsync));
            }
        });
        ServerNotification.OnUserCreated += OnUserCreated;
        ServerNotification.OnUserDeleted += OnUserDeleted;
    }

    ~UsersPageViewModel()
    {
        ServerNotification.OnUserCreated -= OnUserCreated;
        ServerNotification.OnUserDeleted -= OnUserDeleted;
    }

    private void OnUserDeleted(long userId)
    {
        UserDTO? user = Users.FirstOrDefault(u => u.Id == userId);
        if (user == null) return;
        Dispatcher.UIThread.Post(() => Users.Remove(user));
    }

    private void OnUserCreated(UserDTO user)
    {
        Dispatcher.UIThread.Post(() => Users.Add(user));
    }

    public void DeleteUser(UserDTO user)
    {
        Task.Run(async () =>
        {
            if (!await ManaxApiUserClient.DeleteUserAsync(user.Id))
            {
                InfoEmitted?.Invoke("Failed to delete user", "Error");
                Logger.LogFailure("Failed to delete user"+ user.Id,Environment.StackTrace);
            }
        });
    }

    public void CreateUser()
    {
        Task.Run(async () =>
        {
            UserCreateDTO user = new() { Username = "user", Password = "user" };
            long? id = await ManaxApiUserClient.PostUserAsync(user);
            if (id == null)
            {
                InfoEmitted?.Invoke(this,"Failed to create user");
                Logger.LogFailure("Failed to create user", Environment.StackTrace);
            }
        });
        Task.Run(async () =>
        {
            UserCreateDTO user = new() { Username = "admin", Password = "admin", Role = UserRole.Admin };
            long? id = await ManaxApiUserClient.PostUserAsync(user);
            if (id == null)
            {
                InfoEmitted?.Invoke(this,"Failed to create admin user");
                Logger.LogFailure("Failed to create admin user", Environment.StackTrace);
            }
        });
    }
}