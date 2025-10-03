using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.ViewModels.Popup.ConfirmCancel;
using ManaxClient.ViewModels.Popup.ConfirmCancel.Content;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.User;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.ViewModels.Pages.User;

public partial class UsersPageViewModel : PageViewModel
{
    [ObservableProperty] private ObservableCollection<Models.User> _users = [];

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

                Dispatcher.UIThread.Post(() => Users.Add(new Models.User(userResponse.GetValue())));
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
        Models.User? user = Users.FirstOrDefault(u => u.Id == userId);
        if (user == null) return;
        Dispatcher.UIThread.Post(() => Users.Remove(user));
    }

    private void OnUserCreated(UserDto user)
    {
        Dispatcher.UIThread.Post(() => Users.Add(new Models.User(user)));
    }

    public void DeleteUser(Models.User user)
    {
        Task.Run(async () =>
        {
            Optional<bool> deleteUserResponse = await ManaxApiUserClient.DeleteUserAsync(user.Id);
            InfoEmitted?.Invoke(this, deleteUserResponse.Failed
                ? deleteUserResponse.Error
                : $"User '{user.Username}' was deleted");
        });
    }
    
    public void EditUserPermissions(long userId)
    {
        UserPermissionsEditViewModel content = new(userId);
        ConfirmCancelViewModel context = new(content);
        Controls.Popups.Popup popup = new(context);
        PopupRequested?.Invoke(this, popup);
        popup.Closed += async void (_, _) =>
        {
            try
            {
                if (context.Canceled()) return;
                List<Permission> perms = content.GetSelectedPermissions();
                Optional<bool> postUserResponse = await ManaxApiPermissionClient.SetPermissionsAsync(userId,perms);
                if (postUserResponse.Failed) InfoEmitted?.Invoke(this, postUserResponse.Error);
            }
            catch (Exception e)
            {
                Logger.LogError("Error updating user permissions", e, Environment.StackTrace);
                InfoEmitted?.Invoke(this, "Error updating user permissions");
            }
        };
    }

    public void CreateUser()
    {
        UserCreateViewModel content = new();
        ConfirmCancelViewModel context = new(content);
        Controls.Popups.Popup popup = new(context);
        PopupRequested?.Invoke(this, popup);
        popup.Closed += async void (_, _) =>
        {
            try
            {
                if (context.Canceled()) return;
                UserCreateDto user = content.GetResult();
                Optional<bool> postUserResponse = await ManaxApiUserClient.PostUserAsync(user);
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