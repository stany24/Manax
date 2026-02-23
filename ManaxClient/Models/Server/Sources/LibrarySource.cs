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
using ManaxLibrary.DTO.Library;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models.Server.Sources;

public class LibrarySource
{
    private readonly Lock _librariesLock = new();
    private readonly Lock _loadLock = new();
    public readonly SourceCache<Library, long> Libraries = new(x => x.Id);
    private bool _loaded;

    public LibrarySource()
    {
        WeakReferenceMessenger.Default.Register<LoggedInMessage>(this, (_, _) => LoadLibraries());
        NotificationReceiver.OnLibraryCreated += OnLibraryCreated;
        NotificationReceiver.OnLibraryDeleted += OnLibraryDeleted;
    }

    private void OnLibraryDeleted(long id)
    {
        lock (_librariesLock)
        {
            Libraries.RemoveKey(id);
        }
    }

    private void OnLibraryCreated(LibraryDto dto)
    {
        lock (_librariesLock)
        {
            Libraries.AddOrUpdate(new Library(dto));
        }
    }

    private void LoadLibraries()
    {
        Task.Run(() =>
        {
            lock (_loadLock)
            {
                if (_loaded) return;
                try
                {
                    Optional<List<long>> response = ManaxApiLibraryClient.GetLibraryIdsAsync().Result;
                    if (response.Failed)
                    {
                        Logger.LogFailure(response.Error);
                        WeakReferenceMessenger.Default.Send(new NotificationMessage(response.Error));
                        return;
                    }

                    foreach (Optional<LibraryDto> libraryResponse in response.GetValue()
                                 .Select(id => ManaxApiLibraryClient.GetLibraryAsync(id).Result))
                    {
                        if (libraryResponse.Failed)
                        {
                            Logger.LogFailure(libraryResponse.Error);
                            WeakReferenceMessenger.Default.Send(new NotificationMessage(libraryResponse.Error));
                            continue;
                        }

                        lock (_librariesLock)
                        {
                            Libraries.AddOrUpdate(new Library(libraryResponse.GetValue()));
                        }
                    }

                    _loaded = true;
                }
                catch (Exception e)
                {
                    const string error = "Failed to load libraries from server";
                    Logger.LogError(error, e);
                    WeakReferenceMessenger.Default.Send(new NotificationMessage(error));
                }
            }
        });
    }
}