using System.Drawing;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.DTO.Tag;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models.Server.Data;

public partial class Tag : ObservableObject
{
    [ObservableProperty] private Color _color;
    [ObservableProperty] private long _id;
    [ObservableProperty] private string _name = string.Empty;

    public Tag(TagDto dto)
    {
        FromTagDto(dto);
        NotificationReceiver.OnTagUpdated += OnTagUpdated;
    }

    ~Tag()
    {
        NotificationReceiver.OnTagUpdated -= OnTagUpdated;
    }

    public override bool Equals(object? obj)
    {
        return obj is Tag tag && Id == tag.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    private void OnTagUpdated(TagDto tag)
    {
        if (tag.Id != Id) return;
        FromTagDto(tag);
    }

    private void FromTagDto(TagDto dto)
    {
        Id = dto.Id;
        Name = dto.Name;
        Color = Color.FromArgb(dto.ColorArgb);
    }
}