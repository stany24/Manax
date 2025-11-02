using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Aspose.Zip.Rar;
using CommunityToolkit.Mvvm.ComponentModel;
using ImageMagick;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Setting;
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels.Pages.Upload.Tab;

public partial class AutoCleanupTabViewModel:PageViewModel
{
    [ObservableProperty] private string _folderToProcess = string.Empty;
    [ObservableProperty] private int _nbArchive;
    [ObservableProperty] private int _currentArchive;
    [ObservableProperty] private int _nbImage;
    [ObservableProperty] private int _currentImage;
    
    private readonly string[] _formatToDelete = ["xml", "gif", "bin", "js", "css", "html"];
    private readonly List<string> _archivesFormats = ["cbr", "cbz","zip"];
    private readonly string[] _sourceFormats = ["(EN)", "(ALL)"];
    private readonly string[] _imagesFormats = ["jpg", "jpeg", "png", "webp", "heif", "heic","avif"];
    public ObservableCollection<string> Errors { get; } = [];
    private SettingsData? _settings;

    public void Clean()
    {
        MoveSeriesToRoot();
        DecompressFiles();
        ScaleAndConvertImages();
        RemoveUnwantedFiles();
        Task.Run(LoadSettings);
    }

    private void MoveSeriesToRoot()
    {
        IEnumerable<string> sourcesFolders = _sourceFormats.SelectMany(pattern =>
            Directory.GetDirectories(FolderToProcess, "*" + pattern, SearchOption.TopDirectoryOnly));
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
            if (!Directory.Exists(FolderToProcess + mangaName))
            {
                Directory.Move(manga, FolderToProcess + mangaName);
                continue;
            }

            foreach (string file in Directory.GetFiles(manga))
            {
                string fileName = FolderToProcess + mangaName + file.Replace(manga, "");
                File.Move(file, fileName);
            }
            Directory.Delete(manga);
        }
    }
    
    private void DecompressFiles()
    {
        string[] compressedFiles = _archivesFormats
            .SelectMany(ext => Directory.GetFiles(FolderToProcess, "*." + ext, SearchOption.AllDirectories))
            .ToArray();
        NbArchive = compressedFiles.Length;
        Parallel.ForEach(compressedFiles, file =>
        {
            switch (Path.GetExtension(file))
            {
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
            Errors.Add("Failed to extract rar file: " + file + " Error: " + e.Message);
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
            Errors.Add("Failed to extract zip file: " + file + " Error: " + e.Message);
        }
    }

    private void ScaleAndConvertImages()
    {
        string[] imagesToConvert = _imagesFormats.AsParallel().SelectMany(ext =>
            Directory.GetFiles(FolderToProcess, "*." + ext, SearchOption.AllDirectories)).ToArray();
        LoadSettings();
        NbImage = imagesToConvert.Length;
        Parallel.ForEach(imagesToConvert, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            file =>
            {
                try { ConvertImage(file); }
                catch (Exception e)
                {
                    Errors.Add("Failed to convert image: " + file + "Error: " + e.Message);
                }
                CurrentImage++;
            });
    }

    private void ConvertImage(string file)
    {
        if(_settings == null){return;}
        using MagickImage image = new(file);
        if (file.EndsWith("." + _settings.ImageFormat) &&
            image.Quality <= _settings.ImageQuality && image.Width <= _settings.MaxChapterWidth) return;
        if (image.Quality >= _settings.ImageQuality) image.Quality = _settings.ImageQuality;
        if (image.Width > _settings.MaxChapterWidth) image.Resize(_settings.MaxChapterWidth, 0);

        image.HasAlpha = false;
        image.Strip();
        string newFileName = Path.ChangeExtension(file, _settings.ImageFormat.ToString().ToLower());
        File.Delete(file);
        image.Write(newFileName);
    }
    
    private void LoadSettings()
    {
        try
        {
            Optional<SettingsData> settingsAsync = ManaxApiSettingsClient.GetSettingsAsync().Result;
            if (settingsAsync.Failed)
            {
                InfoEmitted?.Invoke(this, "Failed to load settings: " + settingsAsync.Error);
                Logger.LogFailure("Failed to load settings");
                return;
            }

            _settings = settingsAsync.GetValue();
        }
        catch (Exception e)
        {
            InfoEmitted?.Invoke(this, "Error when fetching settings: " + e.Message);
            Logger.LogError("Error when fetching settings", e);
        }
    }

    private void RemoveUnwantedFiles()
    {
        List<string> uselessFiles = _formatToDelete.SelectMany(ext =>
            Directory.EnumerateFiles(FolderToProcess, "*." + ext, SearchOption.AllDirectories)).ToList();
        uselessFiles.ForEach(File.Delete);
        RemoveEmptyFolders(FolderToProcess);
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