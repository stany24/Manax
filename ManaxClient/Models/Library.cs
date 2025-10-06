using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using DynamicData.Binding;
using ManaxClient.Models.Sources;
using ManaxLibrary.DTO.Library;
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
        SortExpressionComparer<Serie> comparer = SortExpressionComparer<Serie>.Ascending(serie => serie.Title);
        SerieSource.Series
            .Connect()
            .AutoRefresh()
            .Filter(serie => serie.LibraryId == Id)
            .SortAndBind(out _series, comparer)
            .Subscribe(changes =>   
            {
                foreach (Change<Serie, long> change in changes)
                {
                    Console.WriteLine("Change detected in library series: " + change);
                    if (change.Reason != ChangeReason.Add) continue;
                    change.Current.LoadInfo();
                    change.Current.LoadPoster();
                }
            });
    }
    
    ~Library()
    {
        ServerNotification.OnLibraryUpdated -= OnLibraryUpdated;
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
}