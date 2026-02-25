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

public class TagSource
{
    private readonly Lock _loadLock = new();
    private readonly Lock _tagLock = new();
    public readonly SourceCache<Tag, long> Tags = new(x => x.Id);
    private bool _loaded;

    public TagSource()
    {
        WeakReferenceMessenger.Default.Register<LoggedInMessage>(this, (_, _) => LoadTags());
        NotificationReceiver.OnTagCreated += OnTagCreated;
        NotificationReceiver.OnTagDeleted += OnTagDeleted;
    }

    private void LoadTags()
    {
        Task.Run(() =>
        {
            try
            {
                lock (_loadLock)
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

                    lock (_tagLock)
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

    private void OnTagDeleted(long id)
    {
        lock (_tagLock)
        {
            Tags.RemoveKey(id);
        }
    }

    private void OnTagCreated(TagDto tag)
    {
        lock (_tagLock)
        {
            Tags.AddOrUpdate(new Tag(tag));
        }
    }
}