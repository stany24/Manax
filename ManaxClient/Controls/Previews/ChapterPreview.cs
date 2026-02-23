using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Messaging;
using Jeek.Avalonia.Localization;
using ManaxClient.Controls.Popups;
using ManaxClient.Event;
using ManaxClient.ViewModels.Pages.Chapter;
using ManaxClient.ViewModels.Popup.ConfirmCancel;
using ManaxClient.ViewModels.Popup.ConfirmCancel.Content;
using ManaxClient.ViewModels.Popup.SelectChoice;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxLibrary.DTO.Read;
using ManaxLibrary.Logging;
using Chapter = ManaxClient.Models.Server.Data.Chapter;

namespace ManaxClient.Controls.Previews;

public class ChapterPreview : Button
{
    public static readonly AttachedProperty<Chapter?> ChapterProperty =
        AvaloniaProperty.RegisterAttached<ChapterPreview, ChapterPreview, Chapter?>(
            "Chapter", null, false, BindingMode.OneTime);

    public ChapterPreview()
    {
        HorizontalAlignment = HorizontalAlignment.Stretch;
        HorizontalContentAlignment = HorizontalAlignment.Stretch;

        Click += (_, _) =>
        {
            Chapter? chapter = GetChapter(this);
            if (chapter == null) return;
            ChapterPageViewModel chapterPageViewModel = new(chapter);
            WeakReferenceMessenger.Default.Send(new PageChangeMessage(chapterPageViewModel));
        };

        Border border = new()
        {
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8)
        };

        Grid mainGrid = new()
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto,Auto,Auto"),
            ColumnSpacing = 12
        };

        Border statusIndicator = new()
        {
            Width = 8,
            Height = 8,
            CornerRadius = new CornerRadius(4),
            VerticalAlignment = VerticalAlignment.Center
        };
        statusIndicator.SetValue(Grid.ColumnProperty, 0);

        statusIndicator.Bind(Border.BackgroundProperty, new Binding(nameof(Chapter) + "." + nameof(Chapter.Read))
        {
            Source = this,
            Mode = BindingMode.OneWay,
            Converter = new FuncValueConverter<ReadDto?, IBrush>(read =>
            {
                if (read == null) return new SolidColorBrush(Color.Parse("#6C757D"));
                return read.Page + 1 == Chapter?.PageNumber
                    ? new SolidColorBrush(Color.Parse("#28A745"))
                    : new SolidColorBrush(Color.Parse("#007ACC"));
            })
        });

        StackPanel infoStack = new()
        {
            Orientation = Orientation.Vertical,
            Spacing = 4,
            VerticalAlignment = VerticalAlignment.Center
        };
        infoStack.SetValue(Grid.ColumnProperty, 1);

        TextBlock chapterName = new()
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.Medium,
            FontSize = 14,
            TextTrimming = TextTrimming.CharacterEllipsis
        };

        TextBlock chapterDetails = new()
        {
            FontSize = 12
        };

        chapterName.Bind(TextBlock.TextProperty, new Binding(nameof(Chapter) + "." + nameof(Chapter.Number))
        {
            Source = this,
            Mode = BindingMode.OneWay,
            Converter = new FuncValueConverter<uint, string>(chapterNumber => $"Chapitre {chapterNumber}")
        });

        chapterDetails.Bind(TextBlock.TextProperty, new Binding(nameof(Chapter) + "." + nameof(Chapter.PageNumber))
        {
            Source = this,
            Mode = BindingMode.OneWay,
            Converter = new FuncValueConverter<uint, string>(pages => $"{pages} page(s)")
        });

        Border progressBadge = new()
        {
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(8, 4),
            VerticalAlignment = VerticalAlignment.Center
        };
        progressBadge.SetValue(Grid.ColumnProperty, 2);

        TextBlock progressText = new()
        {
            FontSize = 11,
            FontWeight = FontWeight.Medium
        };

        progressText.Bind(TextBlock.TextProperty, new Binding(nameof(Chapter) + "." + nameof(Chapter.Read))
        {
            Source = this,
            Mode = BindingMode.OneWay,
            Converter = new FuncValueConverter<ReadDto?, string>(read =>
            {
                if (read == null) return Localizer.Get("Chapter.NotRead");
                uint currentPage = read.Page + 1;
                uint totalPages = Chapter?.PageNumber ?? 0;
                return currentPage >= totalPages ? Localizer.Get("Chapter.Read") : $"{currentPage}/{totalPages}";
            })
        });

        progressBadge.Child = progressText;

        progressBadge.Bind(Border.BackgroundProperty, new Binding(nameof(Chapter) + "." + nameof(Chapter.Read))
        {
            Source = this,
            Mode = BindingMode.OneWay,
            Converter = new FuncValueConverter<ReadDto?, IBrush>(read =>
            {
                if (read == null) return new SolidColorBrush(Color.Parse("#E9ECEF"));
                return read.Page + 1 >= Chapter?.PageNumber
                    ? new SolidColorBrush(Color.Parse("#D4EDDA"))
                    : new SolidColorBrush(Color.Parse("#CCE5FF"));
            })
        });

        Button actionButton = new()
        {
            Content = "...",
            BorderThickness = new Thickness(0),
            Padding = new Thickness(4),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        actionButton.SetValue(Grid.ColumnProperty, 3);
        actionButton.Click += ShowChoices;


        infoStack.Children.Add(chapterName);
        infoStack.Children.Add(chapterDetails);

        mainGrid.Children.Add(statusIndicator);
        mainGrid.Children.Add(infoStack);
        mainGrid.Children.Add(progressBadge);
        mainGrid.Children.Add(actionButton);

        border.Child = mainGrid;
        Content = border;

        Transitions =
        [
            new BrushTransition
            {
                Property = BackgroundProperty,
                Duration = TimeSpan.FromMilliseconds(150)
            }
        ];
    }

    public Chapter? Chapter
    {
        get => GetChapter(this);
        set => SetChapter(this, value);
    }

    private void ShowChoices(object? sender, RoutedEventArgs e)
    {
        string signalIssue = Localizer.Get("Choice.SignalIssue");
        ChooseActionViewModel viewmodel = new([signalIssue]);
        Popup popup = new(viewmodel);
        popup.Closed += (_, _) =>
        {
            string actionName = viewmodel.GetResult();
            if (actionName == signalIssue) ReportIssue();
        };

        WeakReferenceMessenger.Default.Send(new PopupChangeMessage(popup));
        e.Handled = true;
    }

    private void ReportIssue()
    {
        if (Chapter == null) return;
        CreateChapterIssueViewModel content = new(Chapter.Id);
        ConfirmCancelViewModel viewmodel = new(content);
        Popup popup = new(viewmodel);
        popup.Closed += async void (_, _) =>
        {
            try
            {
                if (viewmodel.Canceled()) return;
                IssueChapterReportedCreateDto issue = content.GetResult();
                ManaxLibrary.Optional<bool> chapterIssueAsync =
                    await ManaxApiIssueClient.CreateChapterIssueAsync(issue);
                if (chapterIssueAsync.Failed)
                    WeakReferenceMessenger.Default.Send(new NotificationMessage(chapterIssueAsync.Error));
            }
            catch (Exception e)
            {
                WeakReferenceMessenger.Default.Send(
                    new NotificationMessage(Localizer.Get("ChapterPreview.ReportFailed")));
                Logger.LogError($"Error while creating chapter issue for chapter {Chapter.Id}", e);
            }
        };
        WeakReferenceMessenger.Default.Send(new PopupChangeMessage(popup));
    }

    public static void SetChapter(AvaloniaObject element, Chapter? chapterValue)
    {
        element.SetValue(ChapterProperty, chapterValue);
    }

    public static Chapter? GetChapter(AvaloniaObject element)
    {
        return element.GetValue(ChapterProperty);
    }
}