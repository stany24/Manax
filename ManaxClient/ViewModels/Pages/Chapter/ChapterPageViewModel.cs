using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels.Pages.Chapter;

public partial class ChapterPageViewModel : PageViewModel
{
    private readonly List<Models.Chapter> _chapters;
    [ObservableProperty] private Models.Chapter _chapter = new();
    [ObservableProperty] private bool _controlBordersVisible;
    [ObservableProperty] private int _currentPage;
    [ObservableProperty] private Vector _scrollOffset = new(0, 0);
    
    [ObservableProperty] private string _pagesText = string.Empty;
    [ObservableProperty] private string _backText = string.Empty;
    [ObservableProperty] private string _previousText = string.Empty;
    [ObservableProperty] private string _chapterText = string.Empty;
    [ObservableProperty] private string _nextText = string.Empty;
    [ObservableProperty] private string _previousPageTooltip = string.Empty;
    [ObservableProperty] private string _nextPageTooltip = string.Empty;

    public ChapterPageViewModel(List<Models.Chapter> chapters, Models.Chapter chapter)
    {
        _chapters = chapters;
        ControlBarVisible = false;
        HasMargin = false;
        Chapter = chapter;
        Chapter.LoadPages();
        PropertyChanged += HandleOffsetChanged;
        
        BindLocalizedStrings();
    }

    private void BindLocalizedStrings()
    {
        Localize(() => PagesText, "ChapterPage.Pages", () => Chapter.Pages.Count);
        Localize(() => BackText, "ChapterPage.Back");
        Localize(() => PreviousText, "ChapterPage.Previous");
        Localize(() => ChapterText, "ChapterPage.Chapter");
        Localize(() => NextText, "ChapterPage.Next");
        Localize(() => PreviousPageTooltip, "ChapterPage.PreviousPage");
        Localize(() => NextPageTooltip, "ChapterPage.NextPage");
    }

    public void ChangeBordersVisibility()
    {
        ControlBordersVisible = !ControlBordersVisible;
    }

    public void NextPage()
    {
        if (CurrentPage + 1 == Chapter.Pages.Count) return;

        ScrollOffset = new Vector(0, ScrollOffset.Y + Chapter.Pages[CurrentPage].Size.Height);
    }

    public void PreviousPage()
    {
        if (CurrentPage == 0) return;

        ScrollOffset = new Vector(0, ScrollOffset.Y - Chapter.Pages[CurrentPage].Size.Height);
    }

    public void PreviousChapter()
    {
        Chapter.MarkAsRead(CurrentPage);
        int index = _chapters.FindIndex(c => c.Id == Chapter.Id);
        if (index == 0) return;
        Chapter.CancelLoadingPages();
        Chapter.Pages.Clear();
        Chapter = _chapters[index - 1];
        Chapter.LoadPages();
        ScrollOffset = new Vector(0, 0);
    }

    public void NextChapter()
    {
        Chapter.MarkAsRead(CurrentPage);
        int index = _chapters.FindIndex(c => c.Id == Chapter.Id);
        if (index + 1 == _chapters.Count) return;
        Chapter.CancelLoadingPages();
        Chapter.Pages.Clear();
        Chapter = _chapters[index + 1];
        Chapter.LoadPages();
        ScrollOffset = new Vector(0, 0);
    }

    private void HandleOffsetChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ScrollOffset)) return;

        double height = 0;
        for (int i = 0; i < Chapter.Pages.Count; i++)
        {
            height += Chapter.Pages[i].Size.Height;
            if (!(height > ScrollOffset.Y)) continue;
            CurrentPage = i;
            Logger.LogInfo(CurrentPage.ToString(CultureInfo.InvariantCulture));
            return;
        }
    }

    public override void OnPageClosed()
    {
        Chapter.MarkAsRead(CurrentPage);
    }
}