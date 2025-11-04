using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using ManaxClient.Models.Upload;
using ManaxClient.ViewModels.Pages.Upload.Tab;

namespace ManaxClient.Views.Pages.Upload.Tab;

public partial class ManualCleanupTabView : UserControl
{
    public ManualCleanupTabView()
    {
        InitializeComponent();
        AddHandler(PointerWheelChangedEvent, PointerWheelChangedHandler, Avalonia.Interactivity.RoutingStrategies.Tunnel);
    }

    private void PointerWheelChangedHandler(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is not ManualCleanupTabViewModel viewModel) return;
        if (viewModel.SelectedChapterFolder == null) return;
        if (!e.KeyModifiers.HasFlag(KeyModifiers.Control)) return;
        viewModel.ChangeRowCount(e.Delta.Y > 0);
    }

    private void PointerPressedHandler(object sender, PointerPressedEventArgs args)
    {
        if (sender is not Button button) return;
        if (button.CommandParameter is not string imagePath) return;
        if (DataContext is not ManualCleanupTabViewModel viewModel) return;
        if (viewModel.SelectedChapterFolder == null) return;

        PointerPoint point = args.GetCurrentPoint(button);
        if (!point.Properties.IsRightButtonPressed) return;
        
        ImageFile? imageFile = viewModel.SelectedChapterFolder.Images
            .FirstOrDefault(img => img.Path == imagePath);
            
        if (imageFile != null)
        {
            viewModel.SelectedChapterFolder.DeleteImage(imageFile);
        }
    }
}