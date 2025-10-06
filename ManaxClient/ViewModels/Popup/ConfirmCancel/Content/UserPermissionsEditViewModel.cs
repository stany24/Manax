using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.User;
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels.Popup.ConfirmCancel.Content;

public partial class UserPermissionsEditViewModel : ConfirmCancelContentViewModel
{
    [ObservableProperty] private ObservableCollection<PermissionItemViewModel> _readPermissions = [];
    [ObservableProperty] private ObservableCollection<PermissionItemViewModel> _writePermissions = [];
    [ObservableProperty] private ObservableCollection<PermissionItemViewModel> _deletePermissions = [];
    [ObservableProperty] private ObservableCollection<PermissionItemViewModel> _otherPermissions = [];
    
    private List<Permission> _currentPermissions = [];
    
    public UserPermissionsEditViewModel(long userId)
    {
        Task.Run(() => LoadUserPermissions(userId));
    }
    
    private async void LoadUserPermissions(long userId)
    {
        try
        {
            Optional<List<Permission>> response = await ManaxApiPermissionClient.GetUserPermissionsAsync(userId);
            if (response.Failed)
            {
                return;
            }

            _currentPermissions = response.GetValue();
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                OrganizePermissions();
                CanConfirm = true;
            });
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to load user permissions: ",e);
        }
    }

    private void OrganizePermissions()
    {
        List<Permission> allPermissions = Enum.GetValues<Permission>().ToList();
        
        ReadPermissions.Clear();
        WritePermissions.Clear();
        DeletePermissions.Clear();
        OtherPermissions.Clear();

        foreach (Permission permission in allPermissions)
        {
            PermissionItemViewModel item = new(permission, _currentPermissions.Contains(permission));
            item.PropertyChanged += (_, _) => CanConfirm = true;

            string name = permission.ToString();
            if (name.StartsWith("Read"))
                ReadPermissions.Add(item);
            else if (name.StartsWith("Write") || name.StartsWith("Upload") || name.StartsWith("Set") || name.StartsWith("Mark"))
                WritePermissions.Add(item);
            else if (name.StartsWith("Delete"))
                DeletePermissions.Add(item);
            else
                OtherPermissions.Add(item);
        }
    }

    public List<Permission> GetSelectedPermissions()
    {
        List<Permission> selected = [];
        
        selected.AddRange(ReadPermissions.Where(p => p.IsEnabled).Select(p => p.Permission));
        selected.AddRange(WritePermissions.Where(p => p.IsEnabled).Select(p => p.Permission));
        selected.AddRange(DeletePermissions.Where(p => p.IsEnabled).Select(p => p.Permission));
        selected.AddRange(OtherPermissions.Where(p => p.IsEnabled).Select(p => p.Permission));
        
        return selected;
    }
}

public partial class PermissionItemViewModel : ObservableObject
{
    [ObservableProperty] private bool _isEnabled;
    
    public Permission Permission { get; }
    public string DisplayName { get; }

    public PermissionItemViewModel(Permission permission, bool isEnabled)
    {
        Permission = permission;
        IsEnabled = isEnabled;
        DisplayName = FormatPermissionName(permission.ToString());
    }

    private static string FormatPermissionName(string name)
    {
        return MyRegex().Replace(name, " $1").Trim();
    }

    [GeneratedRegex("([A-Z])")]
    private static partial Regex MyRegex();
}