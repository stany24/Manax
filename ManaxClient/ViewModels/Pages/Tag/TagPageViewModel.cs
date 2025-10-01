using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using ManaxClient.ViewModels.Popup.ConfirmCancel;
using ManaxClient.ViewModels.Popup.ConfirmCancel.Content;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Tag;
using ManaxLibrary.Notifications;

namespace ManaxClient.ViewModels.Pages.Tag;

public class TagPageViewModel : PageViewModel
{
    public ObservableCollection<Models.Tag.Tag> Tags { get; } = [];
    
    public TagPageViewModel()
    {
        Task.Run(LoadTags);
        ServerNotification.OnTagCreated += OnTagCreated;
        ServerNotification.OnTagDeleted += OnTagDeleted;
    }
    
    ~TagPageViewModel()
    {
        ServerNotification.OnTagCreated -= OnTagCreated;
        ServerNotification.OnTagDeleted -= OnTagDeleted;
    }

    private void OnTagDeleted(long tagId)
    {
        Models.Tag.Tag? found = Tags.FirstOrDefault(t => t.Id == tagId);
        if (found == null) return;
        Tags.Remove(found);
    }

    private void OnTagCreated(TagDto tag)
    {
        if (Tags.Any(t => t.Id == tag.Id)) {return;};
        Tags.Add(new Models.Tag.Tag(tag));
    }

    private async void LoadTags()
    {
        try
        {
            
            Optional<List<TagDto>> response = await ManaxApiTagClient.GetTagsAsync();
            if (response.Failed)
            {
                InfoEmitted?.Invoke(this, "Erreur lors du chargement des tags.");
                return;
            }
            Tags.Clear();
            foreach (TagDto tag in response.GetValue()) Tags.Add(new Models.Tag.Tag(tag));
        }
        catch
        {
            InfoEmitted?.Invoke(this, "Erreur lors du chargement des tags.");
        }
    }

    public void CreateTag()
    {
        TagEditViewModel content = new(new TagUpdateDto { Name = "Nouveau Tag", Color = Color.Blue });
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
                    InfoEmitted?.Invoke(this, "Erreur lors de la création du tag.");
            }
            catch
            {
                InfoEmitted?.Invoke(this, "Erreur lors de la création du tag.");
            }
        };

        PopupRequested?.Invoke(this, popup);
    }

    public void UpdateTag(Models.Tag.Tag tag)
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
                InfoEmitted?.Invoke(this, "Erreur lors de la mise à jour du tag.");
            }
        };

        PopupRequested?.Invoke(this, popup);
    }

    public void DeleteTag(Models.Tag.Tag tag)
    {
        Task.Run(async () =>
        {
            try
            {
                await ManaxApiTagClient.DeleteTagAsync(tag.Id);
            }
            catch
            {
                InfoEmitted?.Invoke(this, "Erreur lors de la suppression du tag.");
            }
        });
    }
}