using System.Drawing;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.DTO.Tag;

namespace ManaxClient.ViewModels.Popup.ConfirmCancel.Content;

public partial class TagEditViewModel : ConfirmCancelContentViewModel
{
    [ObservableProperty] private string _name;
    [ObservableProperty] private Color _color;
    
    private readonly TagDto _originalTag;

    public TagEditViewModel(TagDto tag)
    {
        _originalTag = tag;
        _name = tag.Name;
        _color = tag.Color;
        CanConfirm = true;

        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(Name))
            {
                CanConfirm = !string.IsNullOrWhiteSpace(Name);
            }
        };
    }

    public TagDto GetResult()
    {
        return new TagDto
        {
            Id = _originalTag.Id,
            Name = Name.Trim(),
            Color = Color
        };
    }
}
