using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using DynamicData.Binding;
using Jeek.Avalonia.Localization;
using ManaxClient.Models.Sources;
using ManaxClient.ViewModels.Popup.ConfirmCancel;
using ManaxClient.ViewModels.Popup.ConfirmCancel.Content;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Tag;

namespace ManaxClient.ViewModels.Pages.Tag;

public partial class TagPageViewModel : PageViewModel
{
    private readonly ReadOnlyObservableCollection<Models.Tag> _tags;
    
    [ObservableProperty] private string _tagsCountText = string.Empty;

    public TagPageViewModel()
    {
        SortExpressionComparer<Models.Tag> comparer = SortExpressionComparer<Models.Tag>.Descending(tag => tag.Name);
        TagSource.Tags
            .Connect()
            .SortAndBind(out _tags, comparer)
            .Subscribe();
            
        BindLocalizedStrings();
    }
    
    private void BindLocalizedStrings()
    {
        Localize(() => TagsCountText, "TagPage.Count", () => Tags.Count);
    }

    public ReadOnlyObservableCollection<Models.Tag> Tags => _tags;

    public void CreateTag()
    {
        TagEditViewModel content = new(new TagUpdateDto { Name = Localizer.Get("TagPage.DefaultName"), Color = Color.Blue });
        ConfirmCancelViewModel viewModel = new(content);
        Controls.Popups.Popup popup = new(viewModel);
        popup.Closed += async void (_, _) =>
        {
            try
            {
                if (viewModel.Canceled()) return;
                TagUpdateDto result = content.GetResult();
                TagCreateDto tagCreate = new()
                {
                    Name = result.Name,
                    Color = result.Color
                };

                Optional<bool> request = await ManaxApiTagClient.CreateTagAsync(tagCreate);
                if (request.Failed)
                    InfoEmitted?.Invoke(this, Localizer.Get("TagPage.CreateError"));
            }
            catch
            {
                InfoEmitted?.Invoke(this, Localizer.Get("TagPage.CreateError"));
            }
        };

        PopupRequested?.Invoke(this, popup);
    }

    public void UpdateTag(Models.Tag tag)
    {
        TagUpdateDto update = new()
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color
        };
        TagEditViewModel content = new(update);
        ConfirmCancelViewModel viewModel = new(content);
        Controls.Popups.Popup popup = new(viewModel);

        popup.Closed += async void (_, _) =>
        {
            try
            {
                if (viewModel.Canceled()) return;
                TagUpdateDto result = content.GetResult();
                await ManaxApiTagClient.UpdateTagAsync(result);
            }
            catch
            {
                InfoEmitted?.Invoke(this, Localizer.Get("TagPage.UpdateError"));
            }
        };

        PopupRequested?.Invoke(this, popup);
    }

    public void DeleteTag(Models.Tag tag)
    {
        Task.Run(async () =>
        {
            try
            {
                await ManaxApiTagClient.DeleteTagAsync(tag.Id);
            }
            catch
            {
                InfoEmitted?.Invoke(this, Localizer.Get("TagPage.DeleteError"));
            }
        });
    }
}