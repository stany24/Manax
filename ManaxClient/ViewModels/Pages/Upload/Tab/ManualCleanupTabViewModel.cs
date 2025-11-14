using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Models;
using ManaxClient.Models.Upload;

namespace ManaxClient.ViewModels.Pages.Upload.Tab;

public partial class ManualCleanupTabViewModel:TabViewModel
{
    public ObservableCollection<string> ImagesToEdit { get; set; }= [];
    public ObservableCollection<SerieFolder> SerieFolders { get; set; }
    [ObservableProperty] private ChapterFolder? _selectedChapterFolder;
    [ObservableProperty] private int _nbColumns = 4;
    [ObservableProperty] private Vector _imagesOffset = new(0,0);

    private const int MinColumns = 1;
    private const int MaxColumns = 20;
    
    public ManualCleanupTabViewModel()
    {
        string processingFolder = UploadSettings.ProcessingFolder;
        UploadSettings.SettingsChanged += (_, _) => {processingFolder = UploadSettings.ProcessingFolder;};
        
        SerieFolders = new ObservableCollection<SerieFolder>(
            Directory.GetDirectories(processingFolder)
                .Select(f =>new SerieFolder(f)));
        SelectedChapterFolder = SerieFolders.FirstOrDefault()?.Chapters.FirstOrDefault();
        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(SelectedChapterFolder))
            {
                ImagesOffset = new Vector(0,0);
            }
        };
    }
    
    public void ChangeRowCount(bool increase)
    {
        NbColumns = increase ? Math.Max(MinColumns, NbColumns - 1) : Math.Min(MaxColumns, NbColumns + 1);
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

    public void Edit()
    {
        if (ImagesToEdit.Count == 0) return;
        string args = ImagesToEdit.Aggregate("", (current, image) => current + $"\"{image}\" ");
        System.Diagnostics.Process.Start("gimp",args);
    }
    
    public void Next()
    {
        NextRequested?.Invoke(this, new AutoUploadTabViewModel());
    }
    
    public void NextChapter()
    {
        if (SelectedChapterFolder == null) return;
        SerieFolder? parentSerie = SerieFolders.FirstOrDefault(s => s.Chapters.Contains(SelectedChapterFolder));
        if (parentSerie == null) return;
        int currentIndex = parentSerie.Chapters.IndexOf(SelectedChapterFolder);
        if (currentIndex < parentSerie.Chapters.Count - 1)
        {
            SelectedChapterFolder = parentSerie.Chapters[currentIndex + 1];
            return;
        }
        int parentSerieIndex = SerieFolders.IndexOf(parentSerie);
        if (parentSerieIndex < SerieFolders.Count - 1)
        {
            SelectedChapterFolder = SerieFolders[parentSerieIndex + 1].Chapters.FirstOrDefault();
        }
    }
    
    public void PreviousChapter()
    {
        if (SelectedChapterFolder == null) return;
        SerieFolder? parentSerie = SerieFolders.FirstOrDefault(s => s.Chapters.Contains(SelectedChapterFolder));
        if (parentSerie == null) return;
        int currentIndex = parentSerie.Chapters.IndexOf(SelectedChapterFolder);
        if (currentIndex > 0)
        {
            SelectedChapterFolder = parentSerie.Chapters[currentIndex - 1];
            return;
        }
        int parentSerieIndex = SerieFolders.IndexOf(parentSerie);
        if (parentSerieIndex > 0)
        {
            SelectedChapterFolder = SerieFolders[parentSerieIndex - 1].Chapters.LastOrDefault();
        }
    }

    public void MoveUp()
    {
        ImagesOffset = new Vector(0, 0);
    }
    
    public void MoveDown()
    {
        ImagesOffset = new Vector(double.MaxValue,double.MaxValue);
    }
}