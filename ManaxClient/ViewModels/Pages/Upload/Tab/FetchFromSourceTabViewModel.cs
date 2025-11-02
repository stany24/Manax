using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Models.Upload;

namespace ManaxClient.ViewModels.Pages.Upload.Tab;

public partial class FetchFromSourceTabViewModel:PageViewModel
{
    public ObservableCollection<Source> SourceFolders { get; set; } = [];
    [ObservableProperty] private string _processingFolder = string.Empty;
    [ObservableProperty] private bool _canFetch = true;

    public FetchFromSourceTabViewModel()
    {
        LoadSettings();
        UploadSettings.SettingsChanged += (_, _) => LoadSettings();
    }
    
    private void LoadSettings()
    {
        IEnumerable<Source> sources = UploadSettings.SourceFolders.Select(s => new Source { Path = s });
        SourceFolders = new ObservableCollection<Source>(sources);
        ProcessingFolder = UploadSettings.ProcessingFolder;
    }

    public void Fetch()
    {
        CanFetch = false;
        Task.Run(() =>
        {
            foreach (Source source in SourceFolders)
            {
                source.Fetch(ProcessingFolder);
            }
            CanFetch = true;
        });
    }
}