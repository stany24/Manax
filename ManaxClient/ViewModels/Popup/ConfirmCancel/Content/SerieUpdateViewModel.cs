using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Library;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.DTO.Tag;

namespace ManaxClient.ViewModels.Popup.ConfirmCancel.Content;

public partial class SerieUpdateViewModel : ConfirmCancelContentViewModel
{
    private readonly SerieDto _originalSerie;
    
    public ObservableCollection<LibraryDto> Libraries { get; } = [];
    public ObservableCollection<Status> StatusOptions { get; } = [];
    public ObservableCollection<TagDto> AvailableTags { get; set; }= [];
    public ObservableCollection<TagDto> SelectedTags { get; set; } = [];
    
    [ObservableProperty] private string _description;
    [ObservableProperty] private LibraryDto? _selectedLibrary;
    [ObservableProperty] private Status _selectedStatus;
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _tagSearchText = "";
    [ObservableProperty] private TagDto? _selectedTag;

    public SerieUpdateViewModel(SerieDto serie)
    {
        _originalSerie = serie;
        _title = serie.Title;
        _description = serie.Description;
        _selectedStatus = serie.Status;

        foreach (Status status in Enum.GetValues<Status>())
            StatusOptions.Add(status);

        CanConfirm = true;

        PropertyChanged += (_, args) =>
        {
            switch (args.PropertyName)
            {
                case nameof(Title):
                    CanConfirm = !string.IsNullOrWhiteSpace(Title);
                    break;
                case nameof(SelectedTag):
                    if (SelectedTag != null)
                    {
                        AddTag(SelectedTag);
                        SelectedTag = null;
                    }
                    break;
            }
        };

        _ = LoadLibraries();
        _ = LoadTags();
    }

    public void AddTag(TagDto tag)
    {
        Dispatcher.UIThread.Post(() =>
        {
            SelectedTags.Add(tag);
            TagSearchText = "";
            AvailableTags.Remove(tag);
        });
    }

    public void RemoveTag(TagDto tag)
    {
        Dispatcher.UIThread.Post(() =>
        {
            SelectedTags.Remove(tag);
            AvailableTags.Add(tag);
        });
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
                if (!libraryResponse.Failed) libraries.Add(libraryResponse.GetValue());
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

    private async Task LoadTags()
    {
        Optional<List<TagDto>> tagsResponse = await ManaxApiTagClient.GetTagsAsync();
        if (!tagsResponse.Failed)
        {
            List<TagDto> allTags = tagsResponse.GetValue();
            Dispatcher.UIThread.Post(() =>
            {
                SelectedTags.Clear();
                foreach (TagDto tag in _originalSerie.Tags)
                {
                    SelectedTags.Add(tag);
                    allTags.Remove(tag);
                }
                AvailableTags.Clear();
                foreach (TagDto tag in allTags)
                {
                    AvailableTags.Add(tag);
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
            LibraryId = SelectedLibrary?.Id ?? _originalSerie.LibraryId,
            Tags = SelectedTags.ToList()
        };
    }
}