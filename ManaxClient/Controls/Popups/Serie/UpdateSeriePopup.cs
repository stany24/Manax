using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Library;
using ManaxLibrary.DTO.Serie;

namespace ManaxClient.Controls.Popups.Serie;

public class SerieUpdatePopup : ConfirmCancelPopup, INotifyPropertyChanged
{
    private readonly SerieDto _serie;
    private SerieUpdateDto _updateDto;
    private LibraryDto? _selectedLibrary;

    public ObservableCollection<LibraryDto> Libraries { get; } = [];
    public ObservableCollection<Status> StatusOptions { get; } = [];

    public SerieUpdateDto UpdateDto
    {
        get => _updateDto;
        set
        {
            if (_updateDto == value) return;
            _updateDto = value;
            OnPropertyChanged();
        }
    }

    public LibraryDto? SelectedLibrary
    {
        get => _selectedLibrary;
        set
        {
            if (_selectedLibrary == value) return;
            _selectedLibrary = value;
            UpdateDto.LibraryId = value?.Id < 0 ? null : value?.Id;
            OnPropertyChanged();
        }
    }

    public SerieUpdatePopup(SerieDto serie)
    {
        _serie = serie;
        
        _updateDto = new SerieUpdateDto
        {
            Title = serie.Title,
            Description = serie.Description,
            Status = serie.Status,
            LibraryId = serie.LibraryId
        };

        foreach (Status status in Enum.GetValues<Status>())
        {
            StatusOptions.Add(status);
        }

        DataContext = this;
        _ = LoadLibrariesAsync();
    }

    private async Task LoadLibrariesAsync()
    {
        ManaxLibrary.Optional<List<long>> libraryIdsResult = await ManaxApiLibraryClient.GetLibraryIdsAsync();
        if (libraryIdsResult.Failed) return;

        Libraries.Clear();
        
        LibraryDto noLibrary = new() { Id = -1, Name = "No Library" };
        Libraries.Add(noLibrary);
        
        foreach (long id in libraryIdsResult.GetValue())
        {
            ManaxLibrary.Optional<LibraryDto> libraryResult = await ManaxApiLibraryClient.GetLibraryAsync(id);
            if (!libraryResult.Failed)
            {
                Libraries.Add(libraryResult.GetValue());
            }
        }

        SelectedLibrary = Libraries.FirstOrDefault(l => l.Id == (_serie.LibraryId == 0 ? -1 : _serie.LibraryId)) ?? noLibrary;
    }

    protected override Grid GetFormGrid()
    {
        Grid grid = new()
        {
            Margin = new Thickness(10),
            RowSpacing = 5,
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto,Auto"),
            ColumnDefinitions = new ColumnDefinitions("Auto,10,*")
        };

        TextBlock nameLabel = new()
        {
            Text = "Name:",
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(nameLabel, 0);
        Grid.SetColumn(nameLabel, 0);
        grid.Children.Add(nameLabel);

        TextBox titleBox = new();
        titleBox.Bind(TextBox.TextProperty, new Binding($"{nameof(UpdateDto)}.{nameof(SerieUpdateDto.Title)}") { Mode = BindingMode.TwoWay });
        Grid.SetRow(titleBox, 0);
        Grid.SetColumn(titleBox, 2);
        grid.Children.Add(titleBox);

        TextBlock descriptionLabel = new()
        {
            Text = "Description:",
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(descriptionLabel, 1);
        Grid.SetColumn(descriptionLabel, 0);
        grid.Children.Add(descriptionLabel);

        TextBox descriptionBox = new();
        descriptionBox.Bind(TextBox.TextProperty, new Binding($"{nameof(UpdateDto)}.{nameof(SerieUpdateDto.Description)}") { Mode = BindingMode.TwoWay });
        Grid.SetRow(descriptionBox, 1);
        Grid.SetColumn(descriptionBox, 2);
        grid.Children.Add(descriptionBox);

        TextBlock libraryLabel = new()
        {
            Text = "Library:",
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(libraryLabel, 2);
        Grid.SetColumn(libraryLabel, 0);
        grid.Children.Add(libraryLabel);

        ComboBox libraryComboBox = new();
        libraryComboBox.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(Libraries)));
        libraryComboBox.Bind(SelectingItemsControl.SelectedItemProperty, new Binding(nameof(SelectedLibrary)) { Mode = BindingMode.TwoWay });
        libraryComboBox.DisplayMemberBinding = new Binding(nameof(LibraryDto.Name));
        Grid.SetRow(libraryComboBox, 2);
        Grid.SetColumn(libraryComboBox, 2);
        grid.Children.Add(libraryComboBox);

        // Status field
        TextBlock statusLabel = new()
        {
            Text = "Status:",
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(statusLabel, 3);
        Grid.SetColumn(statusLabel, 0);
        grid.Children.Add(statusLabel);

        ComboBox statusComboBox = new();
        statusComboBox.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(StatusOptions)));
        statusComboBox.Bind(SelectingItemsControl.SelectedItemProperty, new Binding($"{nameof(UpdateDto)}.{nameof(SerieUpdateDto.Status)}") { Mode = BindingMode.TwoWay });
        Grid.SetRow(statusComboBox, 3);
        Grid.SetColumn(statusComboBox, 2);
        grid.Children.Add(statusComboBox);

        return grid;
    }

    protected override void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(UpdateDto.Title.Trim()) || 
            string.IsNullOrEmpty(UpdateDto.Description.Trim()))
            return;

        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    public SerieUpdateDto GetResult()
    {
        return UpdateDto;
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}