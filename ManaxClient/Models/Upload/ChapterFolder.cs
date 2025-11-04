using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ManaxClient.Models.Upload;

public partial class ChapterFolder: ObservableObject
{
    [ObservableProperty] private string _name;
    public ObservableCollection<ImageFile> Images { get; set; }
    [ObservableProperty] private ImageFile? _selectedImage;
    
    private static readonly string TrashPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ManaxClient",
        "Trash");

    public ChapterFolder(string path)
    {
        Name = Path.GetFileName(path);
        List<ImageFile> images = Directory.GetFiles(path)
            .Select(f => new ImageFile(f))
            .OrderBy(i => i.FileName, new NaturalSortComparer())
            .ToList();
        Images = new ObservableCollection<ImageFile>(images);
    }
    
    public void DeleteImage(ImageFile image)
    {
        Images.Remove(image);
        if (!Directory.Exists(Path.Combine(TrashPath,Name)))
        {
            Directory.CreateDirectory(Path.Combine(TrashPath,Name));
        }
        File.Move(image.Path, Path.Combine(TrashPath,Name, Path.GetFileName(image.Path)));
        OnPropertyChanged(nameof(Images));
    }
}