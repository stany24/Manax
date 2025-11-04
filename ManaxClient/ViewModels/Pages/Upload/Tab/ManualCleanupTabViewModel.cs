using System;
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
    [ObservableProperty] private int _nbColumns = 4;
    public ObservableCollection<string> ImagesToEdit { get; set; }= [];
    
    public ManualCleanupTabViewModel()
    {
        string processingFolder = UploadSettings.ProcessingFolder;
        UploadSettings.SettingsChanged += (_, _) => {processingFolder = UploadSettings.ProcessingFolder;};
        
        SerieFolders = new ObservableCollection<SerieFolder>(
            Directory.GetDirectories(processingFolder)
                .Select(f =>new SerieFolder(f)));
        SelectedChapterFolder = SerieFolders.FirstOrDefault()?.Chapters.FirstOrDefault();
    }
    
    public void ChangeRowCount(bool increase)
    {
        const int minColumns = 1;
        const int maxColumns = 20;
        NbColumns = increase ? Math.Max(minColumns, NbColumns - 1) : Math.Min(maxColumns, NbColumns + 1);
    }
    
    public void SetSelectedChapterFolder(ChapterFolder? selectedChapterFolder)
    {
        SelectedChapterFolder = selectedChapterFolder;
    }
    
    public void AddImageToEdit(string imagePath)
    {
        if (!ImagesToEdit.Contains(imagePath))
        {
            ImagesToEdit.Add(imagePath);
        }
        OnPropertyChanged(nameof(ImagesToEdit));
    }
}