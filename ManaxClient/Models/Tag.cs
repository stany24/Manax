using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Tag;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models;

public partial class Tag:ObservableObject
{
    public static readonly SourceCache<Tag, long> Tags = new (x => x.Id);
    private static bool _loaded;
    private static readonly object LoadLock = new();
    private static readonly object TagLock = new();
    
    [ObservableProperty] private long _id;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private Color _color;

    public static EventHandler<string>? ErrorEmitted { get; set; }

    static Tag()
    {
        ServerNotification.OnTagCreated += OnTagCreated;
        ServerNotification.OnTagDeleted += OnTagDeleted;
        LoadTags();
    }
    
    public Tag(TagDto dto)
    {
        FromTagDto(dto);
        ServerNotification.OnTagUpdated += OnTagUpdated;
    }
    
    ~Tag()
    {
        ServerNotification.OnTagUpdated -= OnTagUpdated;
    }
    
    private static void LoadTags()
    {
        Task.Run(() =>
        {
            try
            {
                lock (LoadLock)
                {
                    if (_loaded) return;
                    Optional<List<TagDto>> response = ManaxApiTagClient.GetTagsAsync().Result;
                    if (response.Failed)
                    {
                        const string message = "failed to load tags.";
                        Logger.LogFailure(message);
                        ErrorEmitted?.Invoke(null, message);
                        return;
                    }

                    lock (TagLock)
                    {
                        Tags.AddOrUpdate(response.GetValue().Select(dto => new Tag(dto)));
                    }
                    _loaded = true;
                }
            }
            catch(Exception e)
            {
                const string message = "An error occurred while loading tags.";
                Logger.LogError(message,e);
                ErrorEmitted?.Invoke(null, message);
            }
        });
    }
    
    private static void OnTagDeleted(long id)
    {
        lock (TagLock)
        {
            Tags.RemoveKey(id);
        }
    }

    private static void OnTagCreated(TagDto tag)
    {
        lock (TagLock)
        {
            Tags.AddOrUpdate(new Tag(tag));   
        }
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