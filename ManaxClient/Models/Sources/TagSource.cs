using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DynamicData;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Tag;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models.Sources;

public static class TagSource
{
    public static readonly SourceCache<Tag, long> Tags = new(x => x.Id);
    private static bool _loaded;
    private static readonly object LoadLock = new();
    private static readonly object TagLock = new();

    static TagSource()
    {
        ServerNotification.OnTagCreated += OnTagCreated;
        ServerNotification.OnTagDeleted += OnTagDeleted;
        LoadTags();
    }

    public static EventHandler<string>? ErrorEmitted { get; set; }

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
            catch (Exception e)
            {
                const string message = "An error occurred while loading tags.";
                Logger.LogError(message, e);
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
}