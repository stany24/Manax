using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Aspose.Zip.Rar;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ImageMagick;
using ManaxClient.Models.Upload;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Setting;
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels.Pages.Upload.Tab;

public partial class AutoCleanupTabViewModel : TabViewModel
{
    private readonly List<string> _archivesFormats = ["cbr", "cbz", "zip", "rar"];

    private readonly string[] _formatToDelete = ["xml", "gif", "bin", "js", "css", "html"];
    private readonly string[] _imagesFormats = ["jpg", "jpeg", "png", "webp", "heif", "heic", "avif"];
    private readonly string[] _sourceFormats = ["(EN)", "(ALL)"];
    [ObservableProperty] private int _currentArchive;
    [ObservableProperty] private int _currentImage;
    [ObservableProperty] private bool _isProcessing;
    [ObservableProperty] private int _nbArchive;
    [ObservableProperty] private int _nbImage;
    private string _processingFolder;
    private SettingsDataDto? _settings;

    public AutoCleanupTabViewModel()
    {
        _processingFolder = UploadSettings.ProcessingFolder;
        UploadSettings.SettingsChanged += (_, _) => { _processingFolder = UploadSettings.ProcessingFolder; };
    }

    public ObservableCollection<string> Errors { get; } = [];

    public void Clean()
    {
        IsProcessing = true;
        CurrentArchive = 0;
        CurrentImage = 0;
        Errors.Clear();

        Task.Run(async () =>
        {
            await LoadSettings();
            MoveSeriesToRoot();
            DecompressFiles();
            ScaleAndConvertImages();
            RemoveUnwantedFiles();
            NextRequested?.Invoke(this, new ManualCleanupTabViewModel());
        });
    }

    public void Skip()
    {
        NextRequested?.Invoke(this, new ManualCleanupTabViewModel());
    }

    private void MoveSeriesToRoot()
    {
        IEnumerable<string> sourcesFolders = _sourceFormats.SelectMany(pattern =>
            Directory.GetDirectories(_processingFolder, "*" + pattern, SearchOption.TopDirectoryOnly));
        foreach (string source in sourcesFolders)
        {
            MoveMangaOutOfSource(source);
            Directory.Delete(source);
        }
    }

    private void MoveMangaOutOfSource(string source)
    {
        foreach (string manga in Directory.GetDirectories(source, "*", SearchOption.TopDirectoryOnly))
        {
            string mangaName = manga.Replace(source, "");
            if (!Directory.Exists(_processingFolder + mangaName))
            {
                Directory.Move(manga, _processingFolder + mangaName);
                continue;
            }

            foreach (string file in Directory.GetFiles(manga))
            {
                string fileName = _processingFolder + mangaName + file.Replace(manga, "");
                File.Move(file, fileName);
            }

            Directory.Delete(manga);
        }
    }

    private void DecompressFiles()
    {
        string[] compressedFiles = _archivesFormats
            .SelectMany(ext => Directory.GetFiles(_processingFolder, "*." + ext, SearchOption.AllDirectories))
            .ToArray();
        NbArchive = compressedFiles.Length;
        Parallel.ForEach(compressedFiles, file =>
        {
            switch (Path.GetExtension(file))
            {
                case ".rar":
                case ".cbr":
                    ExtractRarInPlace(file);
                    break;
                case ".zip":
                case ".cbz":
                    ExtractZipInPlace(file);
                    break;
            }

            CurrentArchive++;
        });
    }

    private void ExtractRarInPlace(string file)
    {
        try
        {
            RarArchive archive = new(file);
            archive.ExtractToDirectory(Path.ChangeExtension(file, null));
            File.Delete(file);
        }
        catch (Exception e)
        {
            Dispatcher.UIThread.Post(() => Errors.Add("Failed to extract rar file: " + file + " Error: " + e.Message));
        }
    }

    private void ExtractZipInPlace(string file)
    {
        string extractionPath = Path.ChangeExtension(file, null);
        try
        {
            Directory.CreateDirectory(extractionPath);
            ZipFile.ExtractToDirectory(file, extractionPath);
            File.Delete(file);
        }
        catch (Exception e)
        {
            Dispatcher.UIThread.Post(() => Errors.Add("Failed to extract zip file: " + file + " Error: " + e.Message));
        }
    }

    private void ScaleAndConvertImages()
    {
        string[] imagesToConvert = _imagesFormats.AsParallel().SelectMany(ext =>
            Directory.GetFiles(_processingFolder, "*." + ext, SearchOption.AllDirectories)).ToArray();
        NbImage = imagesToConvert.Length;
        Parallel.ForEach(imagesToConvert, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            file =>
            {
                try
                {
                    ConvertImage(file);
                    CurrentImage++;
                }
                catch (Exception e)
                {
                    Dispatcher.UIThread.Post(() => Errors.Add("Failed to convert image: " + file + "Error: " + e.Message));
                }
            });
    }

    private void ConvertImage(string file)
    {
        if (_settings == null) return;
        using MagickImage image = new(file);
        if (file.EndsWith("." + _settings.ImageFormat, StringComparison.InvariantCulture) &&
            image.Quality <= _settings.ImageQuality && image.Width <= _settings.MaxChapterWidth) return;
        if (image.Quality >= _settings.ImageQuality) image.Quality = _settings.ImageQuality;
        if (image.Width > _settings.MaxChapterWidth) image.Resize(_settings.MaxChapterWidth, 0);

        image.HasAlpha = false;
        image.Strip();
        File.Delete(file);
        MagickImage[] images = SplitBigImage(image);
        SaveImages(images,file);
    }

    private static MagickImage[] SplitBigImage(MagickImage image)
    {
        const uint maxHeight = 2560;
        double ratio = (double)image.Height / image.Width;
        bool needsSplit = ratio > 3 && image.Height >= maxHeight;
        if (!needsSplit) { return [image]; }

        uint nbPart = image.Height / maxHeight;
        
        MagickImage[] parts = new MagickImage[nbPart];
        uint desiredHeight = image.Height / nbPart;
        uint leftover = image.Height % nbPart;
        uint yOffset = 0;
        
        for (int i = 0; i < nbPart; i++)
        {
            uint partHeight = desiredHeight;
            if(i == parts.Length - 1) partHeight += leftover;
            yOffset += partHeight;
            
            parts[i] = new MagickImage(new MagickColor("#000000"),image.Width,partHeight);
            parts[i].CopyPixels(image, new MagickGeometry(0, (int)yOffset, image.Width, partHeight));
        }

        return parts;
    }

    private void SaveImages(MagickImage[] images, string oldFileName)
    {
        if (_settings == null) return;
        string extension = _settings.ImageFormat.ToString().ToLower(CultureInfo.InvariantCulture);
        string newFileName =
            Path.ChangeExtension(oldFileName,extension );
        if (images.Length == 1)
        {
            images[0].Write(newFileName);
            return;
        }
        
        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(newFileName);
        string directory = Path.GetDirectoryName(newFileName) ?? string.Empty;
        for (int i = 0; i < images.Length; i++)
        {
            string partFileName = Path.Combine(directory,
                $"{fileNameWithoutExt}_{i + 1:D3}.{extension}");
            images[i].Write(partFileName);
        }
    }

    private async Task LoadSettings()
    {
        try
        {
            Optional<SettingsDataDto> settingsAsync = await ManaxApiSettingsClient.GetSettingsAsync();
            if (settingsAsync.Failed)
            {
                Logger.LogFailure("Failed to load settings");
                return;
            }

            _settings = settingsAsync.GetValue();
        }
        catch (Exception e)
        {
            Logger.LogError("Error when fetching settings", e);
        }
    }

    private void RemoveUnwantedFiles()
    {
        List<string> uselessFiles = _formatToDelete.SelectMany(ext =>
            Directory.EnumerateFiles(_processingFolder, "*." + ext, SearchOption.AllDirectories)).ToList();
        uselessFiles.ForEach(File.Delete);
        RemoveEmptyFolders(_processingFolder);
    }

    private static void RemoveEmptyFolders(string folders)
    {
        foreach (string folder in Directory.GetDirectories(folders))
        {
            RemoveEmptyFolders(folder);
            if (!Directory.EnumerateFileSystemEntries(folder).Any()) Directory.Delete(folder, false);
        }
    }
}