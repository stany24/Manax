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
using Avalonia.Media;
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

    public SerieUpdatePopup(SerieDto serie) : base("Mettre à jour")
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
        
        LibraryDto noLibrary = new() { Id = -1, Name = "Aucune bibliothèque" };
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
            RowSpacing = 16,
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto,Auto,Auto,Auto"),
            ColumnDefinitions = new ColumnDefinitions("*")
        };

        StackPanel headerPanel = new()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Margin = new Thickness(0, 0, 0, 8)
        };
        
        TextBlock icon = new()
        {
            Text = "📚",
            FontSize = 24,
            VerticalAlignment = VerticalAlignment.Center
        };
        
        StackPanel titleStack = new()
        {
            Orientation = Orientation.Vertical,
            Spacing = 4
        };
        
        TextBlock title = new()
        {
            Text = "Modifier la série",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#212529"))
        };
        
        TextBlock subtitle = new()
        {
            Text = "Modifiez les informations de cette série",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#6C757D"))
        };

        titleStack.Children.Add(title);
        titleStack.Children.Add(subtitle);
        headerPanel.Children.Add(icon);
        headerPanel.Children.Add(titleStack);
        
        Grid.SetRow(headerPanel, 0);
        grid.Children.Add(headerPanel);

        StackPanel titlePanel = new()
        {
            Spacing = 8
        };

        TextBlock titleLabel = new()
        {
            Text = "Titre de la série",
            FontWeight = FontWeight.Medium,
            Foreground = new SolidColorBrush(Color.Parse("#495057"))
        };

        TextBox titleBox = new()
        {
            Watermark = "Entrez le titre de la série...",
            Padding = new Thickness(12),
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.Parse("#DEE2E6")),
            FontSize = 14
        };
        titleBox.Bind(TextBox.TextProperty, new Binding($"{nameof(UpdateDto)}.{nameof(SerieUpdateDto.Title)}") { Mode = BindingMode.TwoWay });

        titlePanel.Children.Add(titleLabel);
        titlePanel.Children.Add(titleBox);
        Grid.SetRow(titlePanel, 1);
        grid.Children.Add(titlePanel);

        StackPanel descriptionPanel = new()
        {
            Spacing = 8
        };

        TextBlock descriptionLabel = new()
        {
            Text = "Description",
            FontWeight = FontWeight.Medium,
            Foreground = new SolidColorBrush(Color.Parse("#495057"))
        };

        TextBox descriptionBox = new()
        {
            Watermark = "Entrez la description de la série...",
            Padding = new Thickness(12),
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.Parse("#DEE2E6")),
            FontSize = 14,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 80
        };
        descriptionBox.Bind(TextBox.TextProperty, new Binding($"{nameof(UpdateDto)}.{nameof(SerieUpdateDto.Description)}") { Mode = BindingMode.TwoWay });

        descriptionPanel.Children.Add(descriptionLabel);
        descriptionPanel.Children.Add(descriptionBox);
        Grid.SetRow(descriptionPanel, 2);
        grid.Children.Add(descriptionPanel);

        Grid optionsGrid = new()
        {
            ColumnDefinitions = new ColumnDefinitions("*,16,*"),
            RowDefinitions = new RowDefinitions("Auto,Auto"),
            RowSpacing = 8
        };

        TextBlock libraryLabel = new()
        {
            Text = "Bibliothèque",
            FontWeight = FontWeight.Medium,
            Foreground = new SolidColorBrush(Color.Parse("#495057"))
        };
        Grid.SetColumn(libraryLabel, 0);
        Grid.SetRow(libraryLabel, 0);

        ComboBox libraryComboBox = new()
        {
            Padding = new Thickness(12),
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.Parse("#DEE2E6")),
            FontSize = 14,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        libraryComboBox.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(Libraries)));
        libraryComboBox.Bind(SelectingItemsControl.SelectedItemProperty, new Binding(nameof(SelectedLibrary)) { Mode = BindingMode.TwoWay });
        libraryComboBox.DisplayMemberBinding = new Binding(nameof(LibraryDto.Name));
        Grid.SetColumn(libraryComboBox, 0);
        Grid.SetRow(libraryComboBox, 1);

        TextBlock statusLabel = new()
        {
            Text = "Statut",
            FontWeight = FontWeight.Medium,
            Foreground = new SolidColorBrush(Color.Parse("#495057"))
        };
        Grid.SetColumn(statusLabel, 2);
        Grid.SetRow(statusLabel, 0);

        ComboBox statusComboBox = new()
        {
            Padding = new Thickness(12),
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.Parse("#DEE2E6")),
            FontSize = 14,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        statusComboBox.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(StatusOptions)));
        statusComboBox.Bind(SelectingItemsControl.SelectedItemProperty, new Binding($"{nameof(UpdateDto)}.{nameof(SerieUpdateDto.Status)}") { Mode = BindingMode.TwoWay });
        Grid.SetColumn(statusComboBox, 2);
        Grid.SetRow(statusComboBox, 1);

        optionsGrid.Children.Add(libraryLabel);
        optionsGrid.Children.Add(libraryComboBox);
        optionsGrid.Children.Add(statusLabel);
        optionsGrid.Children.Add(statusComboBox);
        Grid.SetRow(optionsGrid, 3);
        grid.Children.Add(optionsGrid);

        Border helpBorder = new()
        {
            Background = new SolidColorBrush(Color.Parse("#E3F2FD")),
            BorderBrush = new SolidColorBrush(Color.Parse("#007ACC")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12)
        };

        StackPanel helpPanel = new()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8
        };

        TextBlock helpIcon = new()
        {
            Text = "💡",
            FontSize = 14,
            VerticalAlignment = VerticalAlignment.Top
        };

        TextBlock helpText = new()
        {
            Text = "Vous pouvez modifier le titre, la description, la bibliothèque et le statut de cette série. Les changements seront appliqués immédiatement.",
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#495057"))
        };

        helpPanel.Children.Add(helpIcon);
        helpPanel.Children.Add(helpText);
        helpBorder.Child = helpPanel;

        Grid.SetRow(helpBorder, 4);
        grid.Children.Add(helpBorder);

        ApplyInputStyles(titleBox);
        ApplyInputStyles(descriptionBox);
        ApplyInputStyles(libraryComboBox);
        ApplyInputStyles(statusComboBox);

        return grid;
    }

    private static void ApplyInputStyles(Control control)
    {
        control.PointerEntered += (s, _) =>
        {
            if (s is Control c) SetBorderBrush(c, new SolidColorBrush(Color.Parse("#007ACC")));
        };
        
        control.PointerExited += (s, _) =>
        {
            if (s is Control { IsFocused: false } c) 
                SetBorderBrush(c, new SolidColorBrush(Color.Parse("#DEE2E6")));
        };

        control.GotFocus += (s, _) =>
        {
            if (s is Control c) SetBorderBrush(c, new SolidColorBrush(Color.Parse("#007ACC")));
        };

        control.LostFocus += (s, _) =>
        {
            if (s is Control c) SetBorderBrush(c, new SolidColorBrush(Color.Parse("#DEE2E6")));
        };
    }

    private static void SetBorderBrush(Control control, IBrush brush)
    {
        if (control is TextBox tb) tb.BorderBrush = brush;
        else if (control is ComboBox cb) cb.BorderBrush = brush;
    }

    protected override void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        Canceled = false;
        if (string.IsNullOrEmpty(UpdateDto.Title.Trim()) || 
            string.IsNullOrEmpty(UpdateDto.Description.Trim()))
            return;

        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
    
    protected override void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Canceled = true;
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