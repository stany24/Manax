using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTOs;
using ManaxLibrary.DTOs.User;

namespace ManaxApp.ViewModels.User;

public partial class UsersPageViewModel : PageViewModel
{
    [ObservableProperty] private ObservableCollection<UserDTO> _users = [];

    public UsersPageViewModel()
    {
        ControlBarVisible = true;
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
    }

    public void DeleteUser(UserDTO user)
    {
        Task.Run(async () =>
        {
            if (await ManaxApiUserClient.DeleteUserAsync(user.Id)) Dispatcher.UIThread.Post(() => Users.Remove(user));
        });
    }

    public void CreateUser()
    {
        Task.Run(async () =>
        {
            UserCreateDTO user = new() { Username = "user", Password ="user"};
            long? id = await ManaxApiUserClient.PostUserAsync(user);
            if (id == null) return;
            UserDTO? createdUser = await ManaxApiUserClient.GetUserAsync((long)id);
            if (createdUser == null) return;
            Dispatcher.UIThread.Post(() => Users.Add(createdUser));
        });
        Task.Run(async () =>
        {
            UserCreateDTO user = new() { Username = "admin", Password ="admin", Role = UserRole.Admin };
            long? id = await ManaxApiUserClient.PostUserAsync(user);
            if (id == null) return;
            UserDTO? createdUser = await ManaxApiUserClient.GetUserAsync((long)id);
            if (createdUser == null) return;
            Dispatcher.UIThread.Post(() => Users.Add(createdUser));
        });
    }
}