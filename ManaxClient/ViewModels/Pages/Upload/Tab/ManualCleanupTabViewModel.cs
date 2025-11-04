using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Models.Upload;

namespace ManaxClient.ViewModels.Pages.Upload.Tab;

public partial class ManualCleanupTabViewModel:PageViewModel
{
    public ObservableCollection<SerieFolder> SerieFolders { get; set; }
    [ObservableProperty] private ChapterFolder? _selectedChapterFolder;
    public ObservableCollection<string> ImagesToEdit { get; set; }= [];

    public ManualCleanupTabViewModel()
    {
        string processingFolder = UploadSettings.ProcessingFolder;
        UploadSettings.SettingsChanged += (_, _) => {processingFolder = UploadSettings.ProcessingFolder;};
        
        SerieFolders = new ObservableCollection<SerieFolder>(
            Directory.GetDirectories(processingFolder)
                .Select(f =>new SerieFolder(f)));
        foreach (SerieFolder serieFolder in SerieFolders)
        {
            serieFolder.NewImageToEdit += (_, image) => { ImagesToEdit.Add(image); };
        }
        SelectedChapterFolder = SerieFolders.FirstOrDefault()?.Chapters.FirstOrDefault();
    }

    public void SetSelectedChapterFolder(ChapterFolder? selectedChapterFolder)
    {
        SelectedChapterFolder = selectedChapterFolder;
    }
}