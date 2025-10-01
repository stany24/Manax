using System;
using System.IO;
using System.Windows.Input;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using ManaxClient.Controls.Popups;
using ManaxClient.Models.Chapter;
using ManaxClient.ViewModels.Popup.ConfirmCancel;
using ManaxClient.ViewModels.Popup.ConfirmCancel.Content;
using ManaxClient.ViewModels.Popup.SelectChoice;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxLibrary.DTO.Read;
using ManaxLibrary.Logging;

namespace ManaxClient.Controls.Previews;

public class ChapterPreview : Button
{
    public static readonly AttachedProperty<Chapter> ChapterProperty =
        AvaloniaProperty.RegisterAttached<ChapterPreview, Grid, Chapter>(
            "Chapter", new Chapter(), false, BindingMode.OneTime);

    public static readonly StyledProperty<ICommand?> InfoEmittedCommandProperty =
        AvaloniaProperty.Register<ChapterPreview, ICommand?>(nameof(InfoEmittedCommand));

    public static readonly StyledProperty<ICommand?> PopupRequestedCommandProperty =
        AvaloniaProperty.Register<ChapterPreview, ICommand?>(nameof(PopupRequestedCommand));

    private readonly IBrush _backgroundColor = Brushes.White;
    private readonly IBrush _hoverColor = new SolidColorBrush(Color.Parse("#F8F9FA"));

    private readonly IBrush _readTextColor = new SolidColorBrush(Color.Parse("#6C757D"));
    private readonly IBrush _unreadTextColor = new SolidColorBrush(Color.Parse("#212529"));

    public ChapterPreview()
    {
        Background = Brushes.Transparent;
        BorderThickness = new Thickness(0);
        Padding = new Thickness(0);
        HorizontalAlignment = HorizontalAlignment.Stretch;
        HorizontalContentAlignment = HorizontalAlignment.Stretch;

        Border border = new()
        {
            Background = _backgroundColor,
            BorderBrush = new SolidColorBrush(Color.Parse("#E9ECEF")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16, 12)
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

        statusIndicator.Bind(Border.BackgroundProperty, new Binding(nameof(Chapter)+"."+nameof(Chapter.Read))
        {
            Source = this,
            Mode = BindingMode.OneWay,
            Converter = new FuncValueConverter<ReadDto?, IBrush>(read =>
            {
                if (read == null) return new SolidColorBrush(Color.Parse("#6C757D"));
                return read.Page + 1 == Chapter.PageNumber
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
            FontWeight = FontWeight.Medium,
            FontSize = 14,
            TextTrimming = TextTrimming.CharacterEllipsis
        };

        TextBlock chapterDetails = new()
        {
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#6C757D"))
        };

        chapterName.Bind(TextBlock.TextProperty, new Binding(nameof(Chapter)+"."+nameof(Chapter.FileName))
        {
            Source = this,
            Mode = BindingMode.OneWay,
            Converter = new FuncValueConverter<string, string>(fileName =>
                Path.GetFileNameWithoutExtension(fileName) ?? fileName ?? string.Empty)
        });

        chapterName.Bind(ForegroundProperty, new Binding(nameof(Chapter)+"."+nameof(Chapter.Read))
        {
            Source = this,
            Mode = BindingMode.OneWay,
            Converter = new FuncValueConverter<ReadDto?, IBrush>(read =>
            {
                if (read == null) return _unreadTextColor;
                return read.Page + 1 == Chapter.PageNumber ? _readTextColor : _unreadTextColor;
            })
        });

        chapterDetails.Bind(TextBlock.TextProperty, new Binding(nameof(Chapter)+"."+nameof(Chapter.PageNumber))
        {
            Source = this,
            Mode = BindingMode.OneWay,
            Converter = new FuncValueConverter<int, string>(pages => $"{pages} page(s)")
        });

        Border progressBadge = new()
        {
            Background = new SolidColorBrush(Color.Parse("#E9ECEF")),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(8, 4),
            VerticalAlignment = VerticalAlignment.Center
        };
        progressBadge.SetValue(Grid.ColumnProperty, 2);

        TextBlock progressText = new()
        {
            FontSize = 11,
            FontWeight = FontWeight.Medium,
            Foreground = new SolidColorBrush(Color.Parse("#495057"))
        };

        progressText.Bind(TextBlock.TextProperty, new Binding(nameof(Chapter)+"."+nameof(Chapter.Read))
        {
            Source = this,
            Mode = BindingMode.OneWay,
            Converter = new FuncValueConverter<ReadDto?, string>(read =>
            {
                if (read == null) return "Non lu";
                int currentPage = read.Page + 1;
                int totalPages = Chapter.PageNumber;
                return currentPage == totalPages ? "Terminé" : $"{currentPage}/{totalPages}";
            })
        });

        progressBadge.Child = progressText;

        progressBadge.Bind(Border.BackgroundProperty, new Binding(nameof(Chapter)+"."+nameof(Chapter.Read))
        {
            Source = this,
            Mode = BindingMode.OneWay,
            Converter = new FuncValueConverter<ReadDto?, IBrush>(read =>
            {
                if (read == null) return new SolidColorBrush(Color.Parse("#E9ECEF"));
                return read.Page + 1 == Chapter.PageNumber
                    ? new SolidColorBrush(Color.Parse("#D4EDDA"))
                    : new SolidColorBrush(Color.Parse("#CCE5FF"));
            })
        });

        Button actionButton = new()
        {
            Content = "...",
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(4),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        actionButton.SetValue(Grid.ColumnProperty, 4);
        actionButton.Click += ShowChoices;

        TextBlock chevron = new()
        {
            Text = "›",
            FontSize = 16,
            Foreground = new SolidColorBrush(Color.Parse("#6C757D")),
            VerticalAlignment = VerticalAlignment.Center
        };
        chevron.SetValue(Grid.ColumnProperty, 3);

        infoStack.Children.Add(chapterName);
        infoStack.Children.Add(chapterDetails);

        mainGrid.Children.Add(statusIndicator);
        mainGrid.Children.Add(infoStack);
        mainGrid.Children.Add(progressBadge);
        mainGrid.Children.Add(chevron);
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

    public ICommand? InfoEmittedCommand
    {
        get => GetValue(InfoEmittedCommandProperty);
        set => SetValue(InfoEmittedCommandProperty, value);
    }

    public ICommand? PopupRequestedCommand
    {
        get => GetValue(PopupRequestedCommandProperty);
        set => SetValue(PopupRequestedCommandProperty, value);
    }

    public Chapter Chapter
    {
        get => GetChapter(this);
        set => SetChapter(this, value);
    }

    private void ShowChoices(object? sender, RoutedEventArgs e)
    {
        const string signalIssue = "Signaler un problème";
        ChooseActionViewModel viewmodel = new([signalIssue]);
        Popup popup = new(viewmodel);
        popup.Closed += (_, _) =>
        {
            string actionName = viewmodel.GetResult();
            switch (actionName)
            {
                case signalIssue:
                    ReportIssue();
                    break;
            }
        };

        PopupRequestedCommand?.Execute(popup);
        e.Handled = true;
    }

    private void ReportIssue()
    {
        CreateChapterIssueViewModel content = new(Chapter.Id);
        ConfirmCancelViewModel viewmodel = new(content);
        Popup popup = new(viewmodel);
        popup.Closed += async void (_, _) =>
        {
            try
            {
                if (viewmodel.Canceled()) return;
                ReportedIssueChapterCreateDto issue = content.GetResult();
                ManaxLibrary.Optional<bool> chapterIssueAsync =
                    await ManaxApiIssueClient.CreateChapterIssueAsync(issue);
                if (chapterIssueAsync.Failed)
                    InfoEmittedCommand?.Execute(chapterIssueAsync.Error);
            }
            catch (Exception e)
            {
                InfoEmittedCommand?.Execute(" Une erreur est survenue lors de la création du problème.");
                Logger.LogError("Erreur lors de la création d'un problème de chapitre", e, Environment.StackTrace);
            }
        };
        Dispatcher.UIThread.Post(() => { PopupRequestedCommand?.Execute(popup); });
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        if (Content is Border border) border.Background = _hoverColor;
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        if (Content is Border border) border.Background = _backgroundColor;
    }

    public static void SetChapter(AvaloniaObject element, Chapter serieValue)
    {
        element.SetValue(ChapterProperty, serieValue);
    }

    public static Chapter GetChapter(AvaloniaObject element)
    {
        return element.GetValue(ChapterProperty);
    }
}