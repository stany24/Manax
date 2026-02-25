using System.IO;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ManaxClient.Models.Upload;

public partial class Source : ObservableObject
{
    [ObservableProperty] private int _current;
    [ObservableProperty] private int _fileNumber;
    [ObservableProperty] private string _path = string.Empty;
    [ObservableProperty] private double _percentage;

    public void Fetch(string processingFolder)
    {
        if (!Directory.Exists(Path)) return;
        if (!Directory.Exists(processingFolder)) return;
        CopyAllFolders(processingFolder);
        CopyAllFiles(processingFolder);
        DeleteAllEmptyFolders();
    }

    private void DeleteAllEmptyFolders()
    {
        DeleteEmptyFolder(Path);
    }

    private static void DeleteEmptyFolder(string folder)
    {
        foreach (string subFolder in Directory.GetDirectories(folder)) DeleteEmptyFolder(subFolder);

        if (Directory.GetFiles(folder).Length == 0 && Directory.GetDirectories(folder).Length == 0)
            Directory.Delete(folder, false);
    }

    private void CopyAllFiles(string processingFolder)
    {
        string[] files = Directory.GetFiles(Path, "*", SearchOption.AllDirectories);
        FileNumber = files.Length;

        foreach (string file in files)
        {
            string dest = file.Replace(Path, processingFolder);
            File.Copy(file, dest, true);
            File.Delete(file);
            Dispatcher.UIThread.Post(() =>
            {
                Current++;
                Percentage = Current / (double)FileNumber * 100;
            });
        }
    }

    private void CopyAllFolders(string processingFolder)
    {
        string[] dirs = Directory.GetDirectories(Path, "*", SearchOption.AllDirectories);
        FileNumber = dirs.Length;
        Current = 0;

        foreach (string dir in dirs)
        {
            string dest = dir.Replace(Path, processingFolder);
            Directory.CreateDirectory(dest);
            Current++;
        }
    }
}