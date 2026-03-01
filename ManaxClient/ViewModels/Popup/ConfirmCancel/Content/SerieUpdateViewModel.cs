using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ManaxClient.Event;
using ManaxClient.Localization.Localizer;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.Logging;
using Library = ManaxClient.Models.Server.Data.Library;
using Person = ManaxClient.Models.Server.Data.Person;
using Serie = ManaxClient.Models.Server.Data.Serie;
using Tag = ManaxClient.Models.Server.Data.Tag;

namespace ManaxClient.ViewModels.Popup.ConfirmCancel.Content;

public partial class SerieUpdateViewModel : ConfirmCancelContentViewModel
{
    private readonly Serie _originalSerie;

    [ObservableProperty] private string _description;
    [ObservableProperty] private bool _isFilePickerOpen;
    [ObservableProperty] private string _personSearchText = "";
    [ObservableProperty] private Library? _selectedLibrary;
    [ObservableProperty] private Person? _selectedPerson;
    [ObservableProperty] private Status _selectedStatus;
    [ObservableProperty] private Tag? _selectedTag;
    [ObservableProperty] private string _tagSearchText = "";
    [ObservableProperty] private string _title;

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
                case nameof(SelectedPerson):
                    if (SelectedPerson != null)
                    {
                        AddPerson(SelectedPerson);
                        SelectedPerson = null;
                    }

                    break;
            }
        };

        LoadTags();
        LoadPersons();
        LoadLibraries();
    }

    public ObservableCollection<Library> Libraries { get; } = [];
    public ObservableCollection<Status> StatusOptions { get; } = [];
    public ObservableCollection<Tag> AvailableTags { get; set; } = [];
    public ObservableCollection<Person> AvailablePersons { get; set; } = [];
    public ObservableCollection<Tag> SelectedTags { get; set; } = [];
    public ObservableCollection<Person> SelectedPersons { get; set; } = [];

    private void AddTag(Tag tag)
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

    private void AddPerson(Person person)
    {
        Dispatcher.UIThread.Post(() =>
        {
            SelectedPersons.Add(person);
            PersonSearchText = "";
            AvailablePersons.Remove(person);
        });
    }

    public void RemovePerson(Person person)
    {
        Dispatcher.UIThread.Post(() =>
        {
            SelectedPersons.Remove(person);
            AvailablePersons.Add(person);
        });
    }

    private void LoadLibraries()
    {
        Dispatcher.UIThread.Post(() =>
        {
            foreach (Library library in MainWindowViewModel.Instance.LibrarySource.Libraries.Items.ToList())
            {
                Libraries.Add(library);
                if (library.Id == _originalSerie.LibraryId)
                    SelectedLibrary = library;
            }
        });
    }

    private void LoadTags()
    {
        List<Tag> allTags = MainWindowViewModel.Instance.TagSource.Tags.Items.ToList();
        Dispatcher.UIThread.Post(() =>
        {
            SelectedTags.Clear();
            foreach (Tag tag in _originalSerie.Tags)
            {
                SelectedTags.Add(tag);
                allTags.Remove(tag);
            }

            AvailableTags.Clear();
            foreach (Tag tag in allTags) AvailableTags.Add(tag);
        });
    }

    private void LoadPersons()
    {
        List<Person> allPersons = MainWindowViewModel.Instance.PersonSource.Persons.Items.ToList();
        Dispatcher.UIThread.Post(() =>
        {
            SelectedPersons.Clear();
            foreach (Person tag in _originalSerie.Persons)
            {
                SelectedPersons.Add(tag);
                allPersons.Remove(tag);
            }

            AvailablePersons.Clear();
            foreach (Person tag in allPersons) AvailablePersons.Add(tag);
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
            TagIds = SelectedTags.Select(t => t.Id).ToList(),
            PersonIds = SelectedPersons.Select(p => p.Id).ToList()
        };
    }

    public async void ReplacePoster()
    {
        try
        {
            if (IsFilePickerOpen) return;
            IsFilePickerOpen = true;

            Window? window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;
            if (window?.StorageProvider == null) return;

            IReadOnlyList<IStorageFile> files = await window.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = Localizer.Get("SeriePage.SelectPosterImage"),
                    AllowMultiple = false,
                    FileTypeFilter =
                    [
                        new FilePickerFileType("Images")
                        {
                            Patterns = ["*.jpg", "*.webp", "*.jpeg", "*.png", "*.bmp", "*.gif"]
                        }
                    ]
                });
            IsFilePickerOpen = false;

            if (files.Count == 0) return;
            string filePath = files[0].Path.LocalPath;
            if (string.IsNullOrEmpty(filePath)) return;

            Optional<bool> replacePosterResponse = await ManaxApiUploadClient.ReplacePosterAsync(
                filePath,
                files[0].Name,
                _originalSerie.Id);

            if (replacePosterResponse.Failed)
            {
                WeakReferenceMessenger.Default.Send(new NotificationMessage(replacePosterResponse.Error));
            }
            else
            {
                WeakReferenceMessenger.Default.Send(
                    new NotificationMessage(Localizer.Get("SeriePage.PosterReplacedSuccess")));
                Logger.LogInfo("Poster replaced successfully for serie ID: " + _originalSerie.Id);
            }
        }
        catch (Exception e)
        {
            WeakReferenceMessenger.Default.Send(
                new NotificationMessage(Localizer.Get("SeriePage.ErrorReplacingPoster")));
            Logger.LogError("Error replacing poster for serie ID: " + _originalSerie.Id, e);
        }
    }
}