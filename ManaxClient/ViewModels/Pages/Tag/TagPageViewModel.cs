using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using Jeek.Avalonia.Localization;
using ManaxClient.Models.Server.Sources;
using ManaxClient.ViewModels.Popup.ConfirmCancel;
using ManaxClient.ViewModels.Popup.ConfirmCancel.Content;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Tag;

namespace ManaxClient.ViewModels.Pages.Tag;

public class TagPageViewModel : PageViewModel
{
    private readonly ReadOnlyObservableCollection<Models.Server.Data.Tag> _tags;

    public TagPageViewModel()
    {
        SortExpressionComparer<Models.Server.Data.Tag> comparer = SortExpressionComparer<Models.Server.Data.Tag>.Descending(tag => tag.Name);
        TagSource.Tags
            .Connect()
            .SortAndBind(out _tags, comparer)
            .Subscribe();
    }

    public ReadOnlyObservableCollection<Models.Server.Data.Tag> Tags => _tags;

    public void CreateTag()
    {
        TagEditViewModel content = new(new TagUpdateDto
            { Name = Localizer.Get("TagPage.DefaultName"), Color = Color.Blue });
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
                    ColorArgb = result.Color.ToArgb()
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

    public void UpdateTag(Models.Server.Data.Tag tag)
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
                Optional<bool> response = await ManaxApiTagClient.UpdateTagAsync(result);
                if (response.Failed)
                    InfoEmitted?.Invoke(this, response.Error);
            }
            catch
            {
                InfoEmitted?.Invoke(this, Localizer.Get("TagPage.UpdateError"));
            }
        };

        PopupRequested?.Invoke(this, popup);
    }

    public void DeleteTag(Models.Server.Data.Tag tag)
    {
        Task.Run(async () =>
        {
            try
            {
                Optional<bool> response = await ManaxApiTagClient.DeleteTagAsync(tag.Id);
                if (response.Failed)
                    InfoEmitted?.Invoke(this, response.Error);
            }
            catch
            {
                InfoEmitted?.Invoke(this, Localizer.Get("TagPage.DeleteError"));
            }
        });
    }
}