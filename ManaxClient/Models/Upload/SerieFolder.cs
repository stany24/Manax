using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ManaxClient.Models.Upload;

public partial class SerieFolder: ObservableObject
{
    [ObservableProperty] private string _name;
    public ObservableCollection<ChapterFolder> Chapters { get; set; }
    public EventHandler<string>? NewImageToEdit;

    public SerieFolder(string path)
    {
        Name = Path.GetFileName(path);
        Chapters = new ObservableCollection<ChapterFolder>(
            Directory.GetDirectories(path)
                .Select(d => new ChapterFolder(d)));
        foreach (ChapterFolder chapterFolder in Chapters)
        {
            chapterFolder.NewImageToEdit = NewImageToEdit;
        }
    }

    public void ToEdit(string image)
    {
        NewImageToEdit?.Invoke(this, image);
    }
}