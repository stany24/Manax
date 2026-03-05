using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using DynamicData.Binding;
using ManaxClient.Event;
using ManaxClient.Localization.Localizer;
using ManaxClient.ViewModels.Popup.ConfirmCancel;
using ManaxClient.ViewModels.Popup.ConfirmCancel.Content;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.User;
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels.Pages.User;

public class UsersPageViewModel : PageViewModel
{
    private readonly ReadOnlyObservableCollection<Models.Server.Data.User> _users;

    public UsersPageViewModel()
    {
        SortExpressionComparer<Models.Server.Data.User> comparer =
            SortExpressionComparer<Models.Server.Data.User>.Descending(user => user.Username);
        MainWindowViewModel.Instance.UserSource.Users
            .Connect()
            .SortAndBind(out _users, comparer)
            .Subscribe();
    }

    public ReadOnlyObservableCollection<Models.Server.Data.User> Users => _users;

    public void DeleteUser(Models.Server.Data.User user)
    {
        Task.Run(async () =>
        {
            Optional<bool> deleteUserResponse = await ManaxApiUserClient.DeleteUserAsync(user.Id);
            string error = deleteUserResponse.Failed
                ? deleteUserResponse.Error
                : "UserPage.User.Deleted";
            object[] args = deleteUserResponse.Failed ? [] : [user.Username];
            WeakReferenceMessenger.Default.Send(new NotificationMessage(new Notification(error,args)));
        });
    }

    public void EditUserPermissions(long userId)
    {
        UserPermissionsEditViewModel content = new(userId);
        ConfirmCancelViewModel context = new(content);
        Controls.Popups.Popup popup = new(context);
        WeakReferenceMessenger.Default.Send(new PopupChangeMessage(popup));
        popup.Closed += async void (_, _) =>
        {
            try
            {
                if (context.Canceled()) return;
                List<Permission> perms = content.GetSelectedPermissions();
                Optional<bool> postUserResponse = await ManaxApiPermissionClient.SetPermissionsAsync(userId, perms);
                if (postUserResponse.Failed)
                    WeakReferenceMessenger.Default.Send(
                        new NotificationMessage(new Notification("UserPage.UpdatePermissionsError")));
            }
            catch (Exception e)
            {
                Logger.LogError("Error updating user permissions", e);
                WeakReferenceMessenger.Default.Send(
                    new NotificationMessage(new Notification("UserPage.UpdatePermissionsError")));
            }
        };
    }

    public void CreateUser()
    {
        UserCreateViewModel content = new();
        ConfirmCancelViewModel context = new(content);
        Controls.Popups.Popup popup = new(context);
        WeakReferenceMessenger.Default.Send(new PopupChangeMessage(popup));
        popup.Closed += async void (_, _) =>
        {
            try
            {
                if (context.Canceled()) return;
                UserCreateDto user = content.GetResult();
                Optional<bool> postUserResponse = await ManaxApiUserClient.PostUserAsync(user);
                if (postUserResponse.Failed)
                    WeakReferenceMessenger.Default.Send(new NotificationMessage(new Notification("UserPage.CreateError")));
            }
            catch (Exception e)
            {
                Logger.LogError("Error creating user", e);
                WeakReferenceMessenger.Default.Send(new NotificationMessage(new Notification("UserPage.CreateError")));
            }
        };
    }
}