using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxApi.DTOs;
using ManaxApi.Models.Library;
using ManaxApiClient;
using Path = System.IO.Path;

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
            if (await ManaxApiLibraryClient.DeleteLibraryAsync(library.Id)) Dispatcher.UIThread.Post(() => Libraries.Remove(library));
        });
    }
    
    public void ScanLibrary(LibraryDTO library)
    {
        Task.Run(async () =>
        {
            await ManaxApiScanClient.ScanLibraryAsync(library.Id);
        });
        Task.Run(() =>
        {
            while (true)
            {
                Thread.Sleep(1000);
                Dictionary<string, int>? tasks = ManaxApiScanClient.GetTasksAsync().Result;
                if (tasks == null)
                {
                    Console.WriteLine("null");
                }
                else
                {
                    Console.WriteLine(tasks.Count);
                }
            }
        });
    }


    public void CreateLibrary()
    {
        Task.Run(async () =>
        {
            ManaxApi.Models.Library.Library library = new() { Name = "New Library", Description = "Description", Path = Path.Combine(Directory.GetCurrentDirectory(),"Library") };
            long? id = await ManaxApiLibraryClient.PostLibraryAsync(library);
            if (id == null) return;
            LibraryDTO? createdLibrary = await ManaxApiLibraryClient.GetLibraryAsync((long)id);
            if (createdLibrary == null) return;
            Dispatcher.UIThread.Post(() => Libraries.Add(createdLibrary));
        });
    }
}