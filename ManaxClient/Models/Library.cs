using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using DynamicData.Binding;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Library;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models;

public partial class Library:ObservableObject
{
    [ObservableProperty] private long _id;
    [ObservableProperty] private  string _name = string.Empty;
    [ObservableProperty] private  DateTime _creation;
    
    private readonly ReadOnlyObservableCollection<Serie> _series;
    public ReadOnlyObservableCollection<Serie> Series => _series;
    
    public static EventHandler<string>? ErrorEmitted { get; set; }

    public Library(LibraryDto dto)
    {
        ServerNotification.OnLibraryUpdated += OnLibraryUpdated;
        FromLibraryDto(dto);
        SortExpressionComparer<Serie> comparer = SortExpressionComparer<Serie>.Descending(serie => serie.Title);
        Serie.Series
            .Connect()
            .Filter(serie => serie.LibraryId == Id)
            .SortAndBind(out _series, comparer)
            .Subscribe(changes =>   
            {
                foreach (Change<Serie, long> change in changes)
                {
                    if (change.Reason != ChangeReason.Add) continue;
                    change.Current.LoadInfo();
                    change.Current.LoadPoster();
                }
            });
    }

    public Library(long id):this(new LibraryDto { Id = id })
    {
    }
    
    ~Library()
    {
        ServerNotification.OnLibraryUpdated += OnLibraryUpdated;
    }

    private void FromLibraryDto(LibraryDto dto)
    {
        Id = dto.Id;
        Name = dto.Name;
        Creation = dto.Creation;
    }

    private void OnLibraryUpdated(LibraryDto dto)
    {
        if (dto.Id != Id) return;
        FromLibraryDto(dto);
    }
    
    public void LoadInfo()
    {
        Task.Run(() =>
        {
            try
            {
                Optional<LibraryDto> libraryResponse = ManaxApiLibraryClient.GetLibraryAsync(Id).Result;
                if (libraryResponse.Failed)
                {
                    Logger.LogFailure(libraryResponse.Error);
                    ErrorEmitted?.Invoke(this, libraryResponse.Error);
                    return;
                }

                FromLibraryDto(libraryResponse.GetValue());
            }
            catch (Exception e)
            {
                string error = "Failed to load the library with ID: " + Id;
                Logger.LogError(error, e);
                ErrorEmitted?.Invoke(this, error);
            }
        });
    }
}