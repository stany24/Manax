using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Library;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models.Server.Sources;

public static class LibrarySource
{
    public static readonly SourceCache<Data.Library, long> Libraries = new(x => x.Id);
    private static bool _loaded;
    private static readonly Lock LoadLock = new();
    private static readonly Lock LibrariesLock = new();

    static LibrarySource()
    {
        ServerNotification.OnLibraryCreated += OnLibraryCreated;
        ServerNotification.OnLibraryDeleted += OnLibraryDeleted;
    }

    public static EventHandler<string>? ErrorEmitted { get; set; }

    private static void OnLibraryDeleted(long id)
    {
        lock (LibrariesLock)
        {
            Libraries.RemoveKey(id);
        }
    }

    private static void OnLibraryCreated(LibraryDto dto)
    {
        lock (LibrariesLock)
        {
            Libraries.AddOrUpdate(new Data.Library(dto));
        }
    }

    public static void LoadLibraries()
    {
        Task.Run(() =>
        {
            lock (LoadLock)
            {
                if (_loaded) return;
                try
                {
                    Optional<List<long>> response = ManaxApiLibraryClient.GetLibraryIdsAsync().Result;
                    if (response.Failed)
                    {
                        Logger.LogFailure(response.Error);
                        ErrorEmitted?.Invoke(null, response.Error);
                        return;
                    }

                    foreach (Optional<LibraryDto> libraryResponse in response.GetValue().Select(id => ManaxApiLibraryClient.GetLibraryAsync(id).Result))
                    {
                        if (libraryResponse.Failed)
                        {
                            Logger.LogFailure(libraryResponse.Error);
                            ErrorEmitted?.Invoke(null, libraryResponse.Error);
                            continue;
                        }

                        lock (LibrariesLock)
                        {
                            Libraries.AddOrUpdate(new Data.Library(libraryResponse.GetValue()));
                        }
                    }

                    _loaded = true;
                }
                catch (Exception e)
                {
                    const string error = "Failed to load libraries from server";
                    Logger.LogError(error, e);
                    ErrorEmitted?.Invoke(null, error);
                }
            }
        });
    }
}