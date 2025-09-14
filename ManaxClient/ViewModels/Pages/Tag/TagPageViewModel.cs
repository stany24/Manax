using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ManaxClient.ViewModels.Popup.ConfirmCancel;
using ManaxClient.ViewModels.Popup.ConfirmCancel.Content;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Tag;
using ManaxLibrary.Notifications;

namespace ManaxClient.ViewModels.Pages.Tag;

public class TagPageViewModel: PageViewModel
{
    public ObservableCollection<TagDto> Tags { get; } = [];
    
    public TagPageViewModel()
    {
        Task.Run(LoadTags);
        ServerNotification.OnTagCreated += OnTagCreated;
        ServerNotification.OnTagUpdated += OnTagUpdated;
        ServerNotification.OnTagDeleted += OnTagDeleted;
    }

    private void OnTagDeleted(long tagId)
    {
        TagDto? found = Tags.FirstOrDefault(t => t.Id == tagId);
        if (found == null) return;
        Tags.Remove(found);
    }

    private void OnTagUpdated(TagDto tag)
    {
        TagDto? found = Tags.FirstOrDefault(t => t.Id == tag.Id);
        if (found == null) return;
        found.Name = tag.Name;
        found.Color = tag.Color;
    }

    private void OnTagCreated(TagDto tag)
    {
        if (Tags.Any(t => t.Id == tag.Id)) OnTagUpdated(tag);
        Tags.Add(tag);
    }

    private async void LoadTags()
    {
        try
        {
            List<TagDto> tags = await ManaxApiTagClient.GetTagsAsync();
            Tags.Clear();
            foreach (TagDto tag in tags)
            {
                Tags.Add(tag);
            }
        }
        catch
        {
            InfoEmitted?.Invoke(this, "Erreur lors du chargement des tags.");
        }
    }

    public void CreateTag()
    {
        TagEditViewModel content = new(new TagDto { Name = "Nouveau Tag", Color = System.Drawing.Color.Blue });
        ConfirmCancelViewModel viewModel = new(content);
        Controls.Popups.Popup popup = new(viewModel);
        popup.Closed += async (_, _) =>
        {
            if (viewModel.Canceled())
            {
                return;
            }
            TagDto result = content.GetResult();
            try
            {
                TagCreateDto tagCreate = new()
                {
                    Name = result.Name,
                    Color = result.Color
                };
                
                await ManaxApiTagClient.CreateTagAsync(tagCreate);
            }
            catch
            {
                InfoEmitted?.Invoke(this, "Erreur lors de la création du tag.");
            }
        };
        
        PopupRequested?.Invoke(this, popup);
    }

    public void UpdateTag(TagDto tag)
    {
        TagEditViewModel content = new(tag);
        ConfirmCancelViewModel viewModel = new(content);
        Controls.Popups.Popup popup = new(viewModel);
        
        popup.Closed += async (_, _) =>
        {
            if (viewModel.Canceled())
            {
                return;
            }
            TagDto result = content.GetResult();
            try
            {
                await ManaxApiTagClient.UpdateTagAsync(result);
            }
            catch
            {
                InfoEmitted?.Invoke(this, "Erreur lors de la mise à jour du tag.");
            }
        };
        
        PopupRequested?.Invoke(this, popup);
    }

    public void DeleteTag(TagDto tag)
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
