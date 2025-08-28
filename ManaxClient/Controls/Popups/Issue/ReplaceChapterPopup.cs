using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace ManaxClient.Controls.Popups.Issue;

public class ReplaceChapterPopup(string folderPath):ConfirmCancelPopup
{
    public bool CanConfirm = false;
    protected override Grid GetFormGrid()
    {
        Grid grid = new()
        {
            RowDefinitions = new RowDefinitions("Auto"),
            ColumnDefinitions = new ColumnDefinitions("Auto")
        };

        TextBlock infoText = new()
        {
            Text = "When you have finished modifying the chapter at: "+folderPath+ " you can confirm the replacement of the chapter.",
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        infoText.Bind(IsEnabledProperty, new Binding("CanConfirm") { Source = this });
        Grid.SetRow(infoText, 0);
        Grid.SetColumn(infoText, 0);
        
        grid.Children.Add(infoText);
        return grid;
    }

    protected override void OkButtonClicked(object? sender, RoutedEventArgs e)
    {
        Canceled = false;
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
    
    protected override void CancelButtonClicked(object? sender, RoutedEventArgs e)
    {
        if(!CanConfirm){return;}
        Canceled = true;
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}