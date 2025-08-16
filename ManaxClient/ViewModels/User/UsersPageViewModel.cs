using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Controls.Popups.User;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.User;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.ViewModels.User;

public partial class UsersPageViewModel : PageViewModel
{
    [ObservableProperty] private ObservableCollection<UserDto> _users = [];

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
                Optional<UserDto> userResponse = await ManaxApiUserClient.GetUserAsync(id);
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
        UserDto? user = Users.FirstOrDefault(u => u.Id == userId);
        if (user == null) return;
        Dispatcher.UIThread.Post(() => Users.Remove(user));
    }

    private void OnUserCreated(UserDto user)
    {
        Dispatcher.UIThread.Post(() => Users.Add(user));
    }

    public void DeleteUser(UserDto user)
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
        UserCreatePopup popup = new(Owner);
        PopupRequested?.Invoke(this, popup);
        popup.CloseRequested += async void (_, _) =>
        {
            try
            {
                popup.Close();
                UserCreateDto? user = popup.GetResult();
                if (user == null) return;
                Optional<long> postUserResponse = await ManaxApiUserClient.PostUserAsync(user);
                if (postUserResponse.Failed) InfoEmitted?.Invoke(this, postUserResponse.Error);
            }
            catch (Exception e)
            {
                Logger.LogError("Error creating user", e, Environment.StackTrace);
                InfoEmitted?.Invoke(this, "Error creating user");
            }
        };
    }
}