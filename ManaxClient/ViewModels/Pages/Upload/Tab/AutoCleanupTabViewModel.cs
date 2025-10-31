using System.Collections.ObjectModel;
using Avalonia.Media;

namespace ManaxClient.ViewModels.Pages.Upload.Tab;

public class AutoCleanupTabViewModel:PageViewModel
{
    public ObservableCollection<IImage> Images = [];

    public void Clean()
    {
        ScaleAndConvertImages();
        RemoveUnwantedFiles();
    }

    private void ScaleAndConvertImages()
    {
    }

    private void RemoveUnwantedFiles()
    {
    }
}