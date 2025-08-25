using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace ManaxClient.Controls.Popups;

public class ChooseActionPopup : Popup
{
    private string _result = string.Empty;

    public ChooseActionPopup(List<string> options)
    {
        Form.Content = CreateContent(options);
    }

    private StackPanel CreateContent(List<string> options)
    {
        StackPanel mainPanel = new()
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Margin = new Thickness(16)
        };

        TextBlock titleText = new()
        {
            Text = "Actions",
            FontSize = 16,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 8),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        mainPanel.Children.Add(titleText);

        foreach (string option in options)
        {
            Button action = new()
            {
                Content = option,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(12, 8)
            };

            action.Click += (_, _) =>
            {
                _result = option;
                CloseRequested?.Invoke(this, EventArgs.Empty);
            };

            action.PointerEntered += (_, _) => { action.Background = new SolidColorBrush(Color.Parse("#F8F9FA")); };

            action.PointerExited += (_, _) => { action.Background = Brushes.Transparent; };

            mainPanel.Children.Add(action);
        }

        return mainPanel;
    }

    public string GetResult()
    {
        return _result;
    }
}