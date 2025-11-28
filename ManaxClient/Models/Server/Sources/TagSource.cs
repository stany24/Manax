using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using ManaxClient.Event;
using ManaxClient.Models.Server.Data;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Tag;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models.Server.Sources;

public static class TagSource
{
    public static readonly SourceCache<Tag, long> Tags = new(x => x.Id);
    private static bool _loaded;
    private static readonly Lock LoadLock = new();
    private static readonly Lock TagLock = new();

    static TagSource()
    {
        NotificationReceiver.OnTagCreated += OnTagCreated;
        NotificationReceiver.OnTagDeleted += OnTagDeleted;
    }

    public static void LoadTags()
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
                        WeakReferenceMessenger.Default.Send(new NotificationMessage(message));
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
                WeakReferenceMessenger.Default.Send(new NotificationMessage(message));
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