using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Models;
using ManaxClient.Models.Sources;
using ManaxLibrary.DTO.Serie;

namespace ManaxClient.ViewModels.Popup.ConfirmCancel.Content;

public partial class SerieUpdateViewModel : ConfirmCancelContentViewModel
{
    private readonly Serie _originalSerie;
    
    public ObservableCollection<Library> Libraries { get; } = [];
    public ObservableCollection<Status> StatusOptions { get; } = [];
    public ObservableCollection<Tag> AvailableTags { get; set; }= [];
    public ObservableCollection<Tag> SelectedTags { get; set; } = [];
    
    [ObservableProperty] private string _description;
    [ObservableProperty] private Library? _selectedLibrary;
    [ObservableProperty] private Status _selectedStatus;
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _tagSearchText = "";
    [ObservableProperty] private Tag? _selectedTag;

    public SerieUpdateViewModel(Serie serie)
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

        LoadTags();
        LoadLibraries();
    }

    public void AddTag(Tag tag)
    {
        Dispatcher.UIThread.Post(() =>
        {
            SelectedTags.Add(tag);
            TagSearchText = "";
            AvailableTags.Remove(tag);
        });
    }

    public void RemoveTag(Tag tag)
    {
        Dispatcher.UIThread.Post(() =>
        {
            SelectedTags.Remove(tag);
            AvailableTags.Add(tag);
        });
    }

    private void LoadLibraries()
    {
        Dispatcher.UIThread.Post(() =>
        {
            foreach (Library library in LibrarySource.Libraries.Items.ToList())
            {
                Libraries.Add(library);
                if (library.Id == _originalSerie.LibraryId)
                    SelectedLibrary = library;
            }
        });
    }

    private void LoadTags()
    {
        List<Tag> allTags = TagSource.Tags.Items.ToList();
        Dispatcher.UIThread.Post(() =>
        {
            SelectedTags.Clear();
            foreach (Tag tag in _originalSerie.Tags)
            {
                SelectedTags.Add(tag);
                allTags.Remove(tag);
            }
            AvailableTags.Clear();
            foreach (Tag tag in allTags)
            {
                AvailableTags.Add(tag);
            }
        });
    }
    
    public SerieUpdateDto GetResult()
    {
        return new SerieUpdateDto
        {
            Title = Title.Trim(),
            Description = Description.Trim(),
            Status = SelectedStatus,
            LibraryId = SelectedLibrary?.Id ?? _originalSerie.LibraryId,
            Tags = SelectedTags.Select(t => t.ToTagDto()).ToList()
        };
    }
}