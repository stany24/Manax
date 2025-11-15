using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ManaxClient.Models.Upload;

public partial class SerieFolder : ObservableObject
{
    [ObservableProperty] private string _name;

    public SerieFolder(string path)
    {
        Name = Path.GetFileName(path);
        Chapters = new ObservableCollection<ChapterFolder>(
            Directory.GetDirectories(path)
                .OrderBy(i => i, new NaturalSortComparer())
                .Select(d => new ChapterFolder(d)));
    }

    public ObservableCollection<ChapterFolder> Chapters { get; set; }
}