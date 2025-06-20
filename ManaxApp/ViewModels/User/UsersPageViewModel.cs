using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxApi.Models.User;

namespace ManaxApp.ViewModels.User;

public partial class UsersPageViewModel:PageViewModel
{
    [ObservableProperty] private ObservableCollection<ManaxApi.Models.User.User> _users = new();
    
    public UsersPageViewModel()
    {
        ControlBarVisible = true;
        Task.Run( async () =>
        {
            List<long>? ids =  await ManaxApiCaller.ManaxApiUserCaller.GetUsersIdsAsync();
            if (ids == null) return;
            foreach (long id in ids)
            {
                ManaxApi.Models.User.User? userAsync = await ManaxApiCaller.ManaxApiUserCaller.GetUserAsync(id);
                if (userAsync == null) continue;
                Dispatcher.UIThread.Post(() => Users.Add(userAsync));
            }
        });
    }
    
    public void DeleteUser(ManaxApi.Models.User.User user)
    {
        Task.Run(async () =>
        {
            if (await ManaxApiCaller.ManaxApiUserCaller.DeleteUserAsync(user.Id))
            {
                Dispatcher.UIThread.Post(() => Users.Remove(user));
            }
        });
    }
    
    public void CreateUser()
    {
        Task.Run(async () =>
        {
            ManaxApi.Models.User.User user = new(){Role = UserRole.User, Username = "test", PasswordHash = "test"};
            UserInfo? userInfo = await ManaxApiCaller.ManaxApiUserCaller.PostUserAsync(user);
            if (userInfo == null){return;}
            Dispatcher.UIThread.Post(() => Users.Add(user));
        });
    }
}