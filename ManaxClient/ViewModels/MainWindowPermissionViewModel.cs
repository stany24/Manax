using System.Collections.Generic;
using System.Reflection;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.User;

namespace ManaxClient.ViewModels;

public partial class MainWindowViewModel
{
    private List<Permission> _permissions = [];
    private async void LoadPermissions()
    {
        Optional<List<Permission>> myPermissionsAsync = await ManaxApiPermissionClient.GetMyPermissionsAsync();
        if (myPermissionsAsync.Failed)
        {
            ShowInfo(myPermissionsAsync.Error);
            return;
        }

        _permissions = myPermissionsAsync.GetValue();
        NotifyAll();
    }

    private void NotifyAll()
    {
        PropertyInfo[] propertyInfos = GetType().GetProperties();
        foreach (PropertyInfo propertyInfo in propertyInfos)
        {
            if (propertyInfo.PropertyType == typeof(bool) && propertyInfo.Name.StartsWith("Can"))
            {
                OnPropertyChanged(propertyInfo.Name);
            }
        }
    }
    
    // Series permissions
    public bool CanReadSeries => _permissions.Contains(Permission.ReadSeries);
    public bool CanWriteSeries => _permissions.Contains(Permission.WriteSeries);
    public bool CanDeleteSeries => _permissions.Contains(Permission.DeleteSeries);
    
    // Chapter permissions
    public bool CanReadChapters => _permissions.Contains(Permission.ReadChapters);
    public bool CanUploadChapter => _permissions.Contains(Permission.UploadChapter);
    public bool CanDeleteChapters => _permissions.Contains(Permission.DeleteChapters);
    
    // User permissions
    public bool CanReadUsers => _permissions.Contains(Permission.ReadUsers);
    public bool CanWriteUsers => _permissions.Contains(Permission.WriteUsers);
    public bool CanDeleteUsers => _permissions.Contains(Permission.DeleteUsers);
    public bool CanResetPasswords => _permissions.Contains(Permission.ResetPasswords);
    
    // Issue permissions
    public bool CanReadAllIssues => _permissions.Contains(Permission.ReadAllIssues);
    public bool CanWriteIssues => _permissions.Contains(Permission.WriteIssues);
    public bool CanDeleteIssues => _permissions.Contains(Permission.DeleteIssues);
    
    // Rank permissions
    public bool CanReadRanks => _permissions.Contains(Permission.ReadRanks);
    public bool CanWriteRanks => _permissions.Contains(Permission.WriteRanks);
    public bool CanDeleteRanks => _permissions.Contains(Permission.DeleteRanks);
    public bool CanSetMyRank => _permissions.Contains(Permission.SetMyRank);
    
    // Server settings permissions
    public bool CanReadServerSettings => _permissions.Contains(Permission.ReadServerSettings);
    public bool CanWriteServerSettings => _permissions.Contains(Permission.WriteServerSettings);
    
    // Stats permissions
    public bool CanReadServerStats => _permissions.Contains(Permission.ReadServerStats);
    public bool CanReadSelfStats => _permissions.Contains(Permission.ReadSelfStats);
    
    // Save points permissions
    public bool CanReadSavePoints => _permissions.Contains(Permission.ReadSavePoints);
    public bool CanWriteSavePoints => _permissions.Contains(Permission.WriteSavePoints);
    
    // Library permissions
    public bool CanReadLibraries => _permissions.Contains(Permission.ReadLibraries);
    public bool CanWriteLibraries => _permissions.Contains(Permission.WriteLibraries);
    public bool CanDeleteLibraries => _permissions.Contains(Permission.DeleteLibraries);
    
    // Chapter marking permission
    public bool CanMarkChapterAsRead => _permissions.Contains(Permission.MarkChapterAsRead);
    
    // Tag permissions
    public bool CanReadTags => _permissions.Contains(Permission.ReadTags);
    public bool CanWriteTags => _permissions.Contains(Permission.WriteTags);
    public bool CanDeleteTags => _permissions.Contains(Permission.DeleteTags);
    public bool CanSetSerieTags => _permissions.Contains(Permission.SetSerieTags);
}