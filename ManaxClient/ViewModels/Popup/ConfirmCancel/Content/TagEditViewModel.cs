using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.DTO.Tag;
using Color = System.Drawing.Color;

namespace ManaxClient.ViewModels.Popup.ConfirmCancel.Content;

public partial class TagEditViewModel : ConfirmCancelContentViewModel
{
    private readonly TagDto _originalTag;
    private Avalonia.Media.Color _color;
    [ObservableProperty] private SolidColorBrush _colorBrush;
    [ObservableProperty] private string _name;

    public TagEditViewModel(TagDto tag)
    {
        _originalTag = tag;
        Name = tag.Name;
        Color = ConvertToAvaloniaColor(tag.Color);
        ColorBrush = new SolidColorBrush(Color);
        CanConfirm = true;

        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(Name)) CanConfirm = !string.IsNullOrWhiteSpace(Name);
        };
    }

    public Avalonia.Media.Color Color
    {
        get => _color;
        set
        {
            SetProperty(ref _color, value);
            ColorBrush = new SolidColorBrush(value);
        }
    }

    public TagDto GetResult()
    {
        return new TagDto
        {
            Id = _originalTag.Id,
            Name = Name.Trim(),
            Color = ConvertToDrawingColor(Color)
        };
    }

    private static Avalonia.Media.Color ConvertToAvaloniaColor(Color color)
    {
        return Avalonia.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
    }

    private static Color ConvertToDrawingColor(Avalonia.Media.Color color)
    {
        return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
    }
}