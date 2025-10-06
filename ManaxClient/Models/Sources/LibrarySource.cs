using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DynamicData;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Library;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models.Sources;

public static class LibrarySource
{
    public static readonly SourceCache<Library, long> Libraries = new (x => x.Id);
    private static bool _loaded;
    private static readonly object LoadLock = new();
    private static readonly object LibrariesLock = new ();
    
    public static EventHandler<string>? ErrorEmitted { get; set; }
    
    static LibrarySource()
    {
        ServerNotification.OnLibraryCreated += OnLibraryCreated;
        ServerNotification.OnLibraryDeleted += OnLibraryDeleted;
        LoadLibraries();
    }
    
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
            Libraries.AddOrUpdate(new Library(dto));
        }
    }

    private static void LoadLibraries()
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
                        ErrorEmitted?.Invoke(null,response.Error);
                        return;
                    }
                    
                    foreach (long id in response.GetValue())
                    {
                        Optional<LibraryDto> libraryResponse = ManaxApiLibraryClient.GetLibraryAsync(id).Result;
                        if (!libraryResponse.Failed) continue;
                        Logger.LogFailure(libraryResponse.Error);
                        ErrorEmitted?.Invoke(null, libraryResponse.Error);
                        return;
                    }

                    lock (LibrariesLock)
                    {
                        Libraries.AddOrUpdate(response.GetValue().Select(dto => new Library(dto)));
                    }
                    _loaded = true;
                }
                catch (Exception e)
                {
                    const string error = "Failed to load libraries from server";
                    Logger.LogError(error,e);
                    ErrorEmitted?.Invoke(null,error);
                }
            }
        });
    }
}