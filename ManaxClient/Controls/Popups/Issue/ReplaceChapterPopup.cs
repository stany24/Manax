using Avalonia.Controls;
using Avalonia.Interactivity;
using ManaxLibrary.DTO.Chapter;

namespace ManaxClient.Controls.Popups.Issue;

public class ReplaceChapterPopup(ChapterDto chapter):ConfirmCancelPopup
{
    protected override Grid GetFormGrid()
    {
        Grid grid = new()
        {
            RowDefinitions = new RowDefinitions("*,10,Auto"),
            ColumnDefinitions = new ColumnDefinitions("*,10,Auto")
        };

        
        
        return grid;
    }

    protected override void OkButtonClicked(object? sender, RoutedEventArgs e)
    {
        
    }
}