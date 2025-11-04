using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ManaxClient.Models.Upload;

public partial class UploadSettingsData:ObservableObject
{
    [ObservableProperty] private string _processingFolder = string.Empty;
    public ObservableCollection<string> SourceFolders { get; init; }= [];
}