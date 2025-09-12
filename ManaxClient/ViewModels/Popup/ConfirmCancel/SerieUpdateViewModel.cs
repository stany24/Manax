using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Library;
using ManaxLibrary.DTO.Serie;

namespace ManaxClient.ViewModels.Popup.ConfirmCancel;

public partial class SerieUpdateViewModel : ConfirmCancelContentViewModel
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _description;
    [ObservableProperty] private Status _selectedStatus;
    [ObservableProperty] private LibraryDto? _selectedLibrary;

    public ObservableCollection<LibraryDto> Libraries { get; } = [];
    public ObservableCollection<Status> StatusOptions { get; } = [];

    private readonly SerieDto _originalSerie;

    public SerieUpdateViewModel(SerieDto serie)
    {
        _originalSerie = serie;
        _title = serie.Title;
        _description = serie.Description;
        _selectedStatus = serie.Status;

        foreach (Status status in System.Enum.GetValues<Status>())
            StatusOptions.Add(status);

        CanConfirm = true;

        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(Title))
            {
                CanConfirm = !string.IsNullOrWhiteSpace(Title);
            }
        };

        _ = LoadLibraries();
    }

    private async Task LoadLibraries()
    {
        Optional<List<long>> libraryIds = await ManaxApiLibraryClient.GetLibraryIdsAsync();
        if (!libraryIds.Failed)
        {
            List<LibraryDto> libraries = [];
            foreach (long id in libraryIds.GetValue())
            {
                Optional<LibraryDto> libraryResponse = await ManaxApiLibraryClient.GetLibraryAsync(id);
                if (!libraryResponse.Failed)
                {
                    libraries.Add(libraryResponse.GetValue());
                }
            }

            Dispatcher.UIThread.Post(() =>
            {
                foreach (LibraryDto library in libraries)
                {
                    Libraries.Add(library);
                    if (library.Id == _originalSerie.LibraryId)
                        SelectedLibrary = library;
                }
            });
        }
    }

    public SerieUpdateDto GetResult()
    {
        return new SerieUpdateDto
        {
            Title = Title.Trim(),
            Description = Description.Trim(),
            Status = SelectedStatus,
            LibraryId = SelectedLibrary?.Id ?? _originalSerie.LibraryId
        };
    }
}
