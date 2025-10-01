using System.Drawing;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.DTO.Tag;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models.Tag;

public partial class Tag:ObservableObject
{
    [ObservableProperty] private long _id;
    [ObservableProperty] private string _name = string.Empty;

    [ObservableProperty] private Color _color;

    public Tag(TagDto dto)
    {
        FromTagDto(dto);
        ServerNotification.OnTagUpdated += OnTagUpdated;
    }
    
    ~Tag()
    {
        ServerNotification.OnTagUpdated -= OnTagUpdated;
    }
    
    private void OnTagUpdated(TagDto tag)
    {
        if(tag.Id != Id) return;
        FromTagDto(tag);
    }

    private void FromTagDto(TagDto dto)
    {
        Id = dto.Id;
        Name = dto.Name;
        Color = Color.FromArgb(dto.ColorArgb);
    }

    public TagDto ToTagDto()
    {
        return new TagDto
        {
            Id = Id,
            Name = Name,
            ColorArgb = Color.ToArgb()
        };
    }
}