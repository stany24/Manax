using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using ManaxLibrary.DTO.Tag;

namespace ManaxClient.Controls.Popups.Tag;

public class TagEditPopup(TagDto tag) : ConfirmCancelPopup("Modifier")
{
    
    protected override Grid GetFormGrid()
    {
        return new Grid();
    }
    
    protected override void OkButtonClicked(object? sender, RoutedEventArgs e)
    {
        Canceled = true;
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    public TagDto GetResult()
    {
        return tag;
    }
}