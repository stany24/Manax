using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTOs.User;
using ManaxLibrary.Notifications;

namespace ManaxClient.ViewModels.User;

public partial class UsersPageViewModel : PageViewModel
{
    [ObservableProperty] private ObservableCollection<UserDTO> _users = [];

    public UsersPageViewModel()
    {
        Task.Run(async () =>
        {
            Optional<List<long>> usersIdsResponse = await ManaxApiUserClient.GetUsersIdsAsync();
            if (usersIdsResponse.Failed)
            {
                InfoEmitted?.Invoke(this, usersIdsResponse.Error);
                return;
            }
            List<long> ids = usersIdsResponse.GetValue();
            foreach (long id in ids)
            {
                Optional<UserDTO> userResponse = await ManaxApiUserClient.GetUserAsync(id);
                if (userResponse.Failed)
                {
                    InfoEmitted?.Invoke(this, userResponse.Error);
                    continue;
                }
                Dispatcher.UIThread.Post(() => Users.Add(userResponse.GetValue()));
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
            Optional<bool> deleteUserResponse = await ManaxApiUserClient.DeleteUserAsync(user.Id);
            InfoEmitted?.Invoke(this, deleteUserResponse.Failed 
                ? deleteUserResponse.Error 
                : $"User '{user.Username}' was deleted");
        });
    }

    public void CreateUser()
    {
        Task.Run(async () =>
        {
            UserCreateDTO user = new() { Username = "user", Password = "user" };
            Optional<long> postUserResponse = await ManaxApiUserClient.PostUserAsync(user);
            InfoEmitted?.Invoke(this, postUserResponse.Failed 
                ? postUserResponse.Error 
                : $"User '{user.Username}' was successfully created");
        });
        Task.Run(async () =>
        {
            UserCreateDTO user = new() { Username = "admin", Password = "admin", Role = UserRole.Admin};
            Optional<long> postUserResponse = await ManaxApiUserClient.PostUserAsync(user);
            InfoEmitted?.Invoke(this, postUserResponse.Failed 
                ? postUserResponse.Error 
                : $"User '{user.Username}' was successfully created");
        });
    }
}