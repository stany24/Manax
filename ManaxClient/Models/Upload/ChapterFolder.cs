using System;
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
    public EventHandler<string>? NewImageToEdit;
    
    private static readonly string TrashPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ManaxClient",
        "Trash");

    public ChapterFolder(string path)
    {
        Name = Path.GetFileName(path);
        Images = new ObservableCollection<ImageFile>(
            Directory.GetFiles(path)
                .Select(f => new ImageFile(f)));
    }

    public void SetSelectedImage(ImageFile image)
    {
        if (SelectedImage == image)
        {
            ToEdit(image);
            return;
        }
        SelectedImage = image;
    }
    
    private void DeleteImage(ImageFile image)
    {
        Images.Remove(image);
        File.Move(image.Path, Path.Combine(TrashPath,Name, Path.GetFileName(image.Path)));
        OnPropertyChanged(nameof(Images));
    }

    private void ToEdit(ImageFile image)
    {
        NewImageToEdit?.Invoke(this, image.Path);
    }
}