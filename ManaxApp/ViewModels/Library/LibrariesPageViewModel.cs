using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxApp.Controls;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTOs.Library;

namespace ManaxApp.ViewModels.Library;

public partial class LibrariesPageViewModel : PageViewModel
{
    [ObservableProperty] private ObservableCollection<LibraryDTO> _libraries = [];

    public LibrariesPageViewModel()
    {
        ControlBarVisible = true;
        Task.Run(async () =>
        {
            List<long>? ids = await ManaxApiLibraryClient.GetLibraryIdsAsync();
            if (ids == null) return;
            foreach (long id in ids)
            {
                LibraryDTO? libraryAsync = await ManaxApiLibraryClient.GetLibraryAsync(id);
                if (libraryAsync == null) continue;
                Dispatcher.UIThread.Post(() => Libraries.Add(libraryAsync));
            }
        });
    }

    public void DeleteLibrary(LibraryDTO library)
    {
        Task.Run(async () =>
        {
            if (await ManaxApiLibraryClient.DeleteLibraryAsync(library.Id))
                Dispatcher.UIThread.Post(() => Libraries.Remove(library));
        });
    }

    public void ShowLibrary(long libraryId)
    {
        PageChangedRequested?.Invoke(this, new LibraryPageViewModel(libraryId));
    }

    public static void ScanLibrary(LibraryDTO library)
    {
        Task.Run(async () => { await ManaxApiScanClient.ScanLibraryAsync(library.Id); });
    }


    public void CreateLibrary()
    {
        LibraryCreatePopup popup = new();
        popup.CloseRequested += async void (_, _) =>
        {
            try
            {
                popup.Close();
                LibraryCreateDTO? library = popup.GetResult();
                if (library == null) return;
                long? id = await ManaxApiLibraryClient.PostLibraryAsync(library);
                if (id == null)
                {
                    InfoEmitted?.Invoke(this, "Library creation failed");
                    return;
                }

                LibraryDTO? createdLibrary = await ManaxApiLibraryClient.GetLibraryAsync((long)id);
                if (createdLibrary == null) return;
                Dispatcher.UIThread.Post(() => Libraries.Add(createdLibrary));
            }
            catch (Exception)
            {
                InfoEmitted?.Invoke(this, "Error creating library");
            }
        };
        PopupRequested?.Invoke(this, popup);
    }
}