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
    [ObservableProperty] private ImageFile? _selectedImage;
    public ObservableCollection<ImageFile> Images { get; set; }
    private readonly List<KeyValuePair<string,string>> _deletedImages = [];
    
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

        string trashImagePath = Path.Combine(TrashPath, Name, Path.GetFileName(image.Path)+Guid.NewGuid());
        File.Move(image.Path, trashImagePath);
        _deletedImages.Add(new KeyValuePair<string, string>(image.Path, trashImagePath));
        OnPropertyChanged(nameof(Images));
    }
    
    public void RestoreLastImage()
    {
        if (_deletedImages.Count == 0) return;
        KeyValuePair<string, string> lastDeletedImage = _deletedImages[^1];
        File.Move(lastDeletedImage.Value, lastDeletedImage.Key);
        ImageFile imageFile = new(lastDeletedImage.Key);
        imageFile.LoadPreview();
        Images.Add(imageFile);
        Images = new ObservableCollection<ImageFile>(Images.OrderBy(i => i.FileName, new NaturalSortComparer()));
        _deletedImages.RemoveAt(_deletedImages.Count - 1);
        OnPropertyChanged(nameof(Images));
    }

    public void LoadImages()
    {
        foreach (ImageFile imageFile in Images)
        {
            imageFile.LoadPreview();
        }
    }

    public void UnloadImages()
    {
        foreach (ImageFile imageFile in Images)
        {
            imageFile.UnloadPreview();
        }
    }
}