using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ManaxLibrary.DTO.Setting;

public class SettingsData : INotifyPropertyChanged
{
    public bool IsValid
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
        }
    }

    public string Issue
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public uint MaxPosterWidth
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            UpdateIssues();
            OnPropertyChanged();
        }
    } = 800;

    public uint MinPosterWidth
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            UpdateIssues();
            OnPropertyChanged();
        }
    } = 720;

    public uint MaxChapterWidth
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            UpdateIssues();
            OnPropertyChanged();
        }
    } = 800;

    public uint MinChapterWidth
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            UpdateIssues();
            OnPropertyChanged();
        }
    } = 720;

    public uint MaxDescriptionLength
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            UpdateIssues();
            OnPropertyChanged();
        }
    } = 1000;

    public uint MinDescriptionLength
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            UpdateIssues();
            OnPropertyChanged();
        }
    } = 100;

    public uint ImageQuality
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            UpdateIssues();
            OnPropertyChanged();
        }
    } = 94;

    public string PosterName
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            UpdateIssues();
            OnPropertyChanged();
        }
    } = "poster";

    public uint PosterQuality
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            UpdateIssues();
            OnPropertyChanged();
        }
    } = 94;

    public ImageFormat PosterFormat
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            UpdateIssues();
            OnPropertyChanged();
        }
    } = ImageFormat.Webp;

    public ImageFormat ImageFormat
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            UpdateIssues();
            OnPropertyChanged();
        }
    } = ImageFormat.Webp;

    public ArchiveFormat ArchiveFormat
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            UpdateIssues();
            OnPropertyChanged();
        }
    } = ArchiveFormat.Cbz;

    public event PropertyChangedEventHandler? PropertyChanged;

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

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}