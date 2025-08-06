using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ManaxLibrary.DTO.Setting;

public class SettingsData : INotifyPropertyChanged
{
    private void UpdateIssues()
    {
        if (MaxPosterWidth < MinPosterWidth)
        {
            Issue = "MaxPosterWidth cannot be less than MinPosterWidth.";
            IsValid = false;
            return;
        }

        if (MaxChapterWidth < MinChapterWidth)
        {
            Issue = "MaxChapterWidth cannot be less than MinChapterWidth.";
            IsValid = false;
            return;
        }

        if (MaxDescriptionLength < MinDescriptionLength)
        {
            Issue = "MaxDescriptionLength cannot be less than MinDescriptionLength.";
            IsValid = false;
            return;
        }

        if (ImageQuality is < 1 or > 100)
        {
            Issue = "ImageQuality must be between 1 and 100.";
            IsValid = false;
            return;
        }

        if (PosterQuality is < 1 or > 100)
        {
            Issue = "PosterQuality must be between 1 and 100.";
            IsValid = false;
            return;
        }
        IsValid = true;
        Issue = string.Empty;
    }
    
    private bool _isValid;
    public bool IsValid
    {
        get => _isValid;
        set
        {
            if (_isValid == value) return;
            _isValid = value;
            OnPropertyChanged();
        }
    }
    
    private string _issue = string.Empty;
    public string Issue
    {
        get => _issue;
        set
        {
            if (_issue == value) return;
            _issue = value;
            OnPropertyChanged();
        }
    }
    
    private uint _maxPosterWidth = 800;
    public uint MaxPosterWidth
    {
        get => _maxPosterWidth;
        set
        {
            if (_maxPosterWidth == value) return;
            _maxPosterWidth = value;
            UpdateIssues();
            OnPropertyChanged();
        }
    }

    private uint _minPosterWidth = 720;
    public uint MinPosterWidth
    {
        get => _minPosterWidth;
        set
        {
            if (_minPosterWidth == value) return;
            _minPosterWidth = value;
            UpdateIssues();
            OnPropertyChanged();
        }
    }

    private uint _maxChapterWidth = 800;
    public uint MaxChapterWidth
    {
        get => _maxChapterWidth;
        set
        {
            if (_maxChapterWidth == value) return;
            _maxChapterWidth = value;
            UpdateIssues();
            OnPropertyChanged();
        }
    }

    private uint _minChapterWidth = 720;
    public uint MinChapterWidth
    {
        get => _minChapterWidth;
        set
        {
            if (_minChapterWidth == value) return;
            _minChapterWidth = value;
            UpdateIssues();
            OnPropertyChanged();
        }
    }

    private uint _maxDescriptionLength = 1000;
    public uint MaxDescriptionLength
    {
        get => _maxDescriptionLength;
        set
        {
            if (_maxDescriptionLength == value) return;
            _maxDescriptionLength = value;
            UpdateIssues();
            OnPropertyChanged();
        }
    }

    private uint _minDescriptionLength = 100;
    public uint MinDescriptionLength
    {
        get => _minDescriptionLength;
        set
        {
            if (_minDescriptionLength == value) return;
            _minDescriptionLength = value;
            UpdateIssues();
            OnPropertyChanged();
        }
    }

    private uint _imageQuality = 94;
    public uint ImageQuality
    {
        get => _imageQuality;
        set
        {
            if (_imageQuality == value) return;
            _imageQuality = value;
            UpdateIssues();
            OnPropertyChanged();
        }
    }

    private string _posterName = "poster";
    public string PosterName
    {
        get => _posterName;
        set
        {
            if (_posterName == value) return;
            _posterName = value;
            UpdateIssues();
            OnPropertyChanged();
        }
    }

    private uint _posterQuality = 94;
    public uint PosterQuality
    {
        get => _posterQuality;
        set
        {
            if (_posterQuality == value) return;
            _posterQuality = value;
            UpdateIssues();
            OnPropertyChanged();
        }
    }

    private ImageFormat _posterFormat = ImageFormat.Webp;
    public ImageFormat PosterFormat
    {
        get => _posterFormat;
        set
        {
            if (_posterFormat == value) return;
            _posterFormat = value;
            UpdateIssues();
            OnPropertyChanged();
        }
    }

    private ImageFormat _imageFormat = ImageFormat.Webp;
    public ImageFormat ImageFormat
    {
        get => _imageFormat;
        set
        {
            if (_imageFormat == value) return;
            _imageFormat = value;
            UpdateIssues();
            OnPropertyChanged();
        }
    }

    private ArchiveFormat _archiveFormat = ArchiveFormat.Cbz;
    public ArchiveFormat ArchiveFormat
    {
        get => _archiveFormat;
        set
        {
            if (_archiveFormat == value) return;
            _archiveFormat = value;
            UpdateIssues();
            OnPropertyChanged();
        }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}