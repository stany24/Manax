using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ManaxClient.Models.Upload;

public partial class Source:ObservableObject
{
    [ObservableProperty] private string _path = string.Empty;
    [ObservableProperty] private int _fileNumber;
    [ObservableProperty] private int _current;

    public void Fetch(string processingFolder)
    {
        if (!Directory.Exists(Path)) { return; }
        if (!Directory.Exists(processingFolder)) { return; }
        CopyAllFolders(processingFolder);
        CopyAllFiles(processingFolder);
        DeleteAllEmptyFolders();
    }

    private void DeleteAllEmptyFolders()
    {
        string[] dirs = Directory.GetDirectories(Path, "*", SearchOption.AllDirectories);
        foreach (string dir in dirs)
        {
            if (Directory.GetFiles(dir).Length == 0 && Directory.GetDirectories(dir).Length == 0)
            {
                Directory.Delete(dir, false);
            }
        }
    }

    private void CopyAllFiles(string processingFolder)
    {
        string[] files = Directory.GetFiles(Path, "*", SearchOption.AllDirectories);
        FileNumber = files.Length;
        Current = 0;

        foreach (string file in files)
        {
            string dest = file.Replace(Path, processingFolder);
            File.Copy(file, dest, true);
            File.Delete(file);
            Current++;
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