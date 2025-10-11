using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using DynamicData.Binding;
using Jeek.Avalonia.Localization;
using ManaxClient.Models.Sources;
using ManaxClient.ViewModels.Popup.ConfirmCancel;
using ManaxClient.ViewModels.Popup.ConfirmCancel.Content;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.User;
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels.Pages.User;

public partial class UsersPageViewModel : PageViewModel
{
    private readonly ReadOnlyObservableCollection<Models.User> _users;
    
    [ObservableProperty] private string _usersCountText = string.Empty;

    public UsersPageViewModel()
    {
        SortExpressionComparer<Models.User> comparer =
            SortExpressionComparer<Models.User>.Descending(user => user.Username);
        UserSource.Users
            .Connect()
            .SortAndBind(out _users, comparer)
            .Subscribe();
            
        BindLocalizedStrings();
    }
    
    private void BindLocalizedStrings()
    {
        Localize(() => UsersCountText, "UserPage.Count", () => Users.Count);
    }

    public ReadOnlyObservableCollection<Models.User> Users => _users;

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
                Optional<bool> postUserResponse = await ManaxApiPermissionClient.SetPermissionsAsync(userId, perms);
                if (postUserResponse.Failed) InfoEmitted?.Invoke(this, Localizer.Get("UserPage.UpdatePermissionsError"));
            }
            catch (Exception e)
            {
                Logger.LogError("Error updating user permissions", e);
                InfoEmitted?.Invoke(this, Localizer.Get("UserPage.UpdatePermissionsError"));
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
                if (postUserResponse.Failed) InfoEmitted?.Invoke(this, Localizer.Get("UserPage.CreateError"));
            }
            catch (Exception e)
            {
                Logger.LogError("Error creating user", e);
                InfoEmitted?.Invoke(this, Localizer.Get("UserPage.CreateError"));
            }
        };
    }
}