using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Models;
using ManaxClient.Models.Upload;

namespace ManaxClient.ViewModels.Pages.Upload.Tab;

public partial class FetchFromSourceTabViewModel : TabViewModel
{
    [ObservableProperty] private bool _canFetch = true;
    [ObservableProperty] private string _processingFolder = string.Empty;
    [ObservableProperty] private int _sourceCompletedCount;
    [ObservableProperty] private int _sourceInProgressCount;

    public FetchFromSourceTabViewModel()
    {
        LoadSettings();
        UploadSettings.SettingsChanged += (_, _) => LoadSettings();
    }

    public ObservableCollection<Source> SourceFolders { get; set; } = [];

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
                SourceInProgressCount++;
                source.Fetch(ProcessingFolder);
                SourceCompletedCount++;
                SourceInProgressCount--;
            }

            CanFetch = true;
            NextRequested?.Invoke(this, new AutoCleanupTabViewModel());
        });
    }

    public void Skip()
    {
        NextRequested?.Invoke(this, new AutoCleanupTabViewModel());
    }
}