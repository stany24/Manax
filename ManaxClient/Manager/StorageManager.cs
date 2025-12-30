using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ManaxLibrary.Logging;

namespace ManaxClient.Manager;

public static class StorageManager
{
    public static readonly string ConfigFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ManaxClient");
    public static readonly string TrashFolder = Path.Combine(ConfigFolder,"Trash");
    public static readonly string LoginFile = Path.Combine(ConfigFolder,"login.json");
    public static readonly string ThemeFile = Path.Combine(ConfigFolder,"themesettings.json");
    public static readonly string UploadFile = Path.Combine(ConfigFolder,"uploadsettings.json");

    static StorageManager()
    {
        if(!Directory.Exists(ConfigFolder)){Directory.CreateDirectory(ConfigFolder);}
        if(!Directory.Exists(TrashFolder)){Directory.CreateDirectory(TrashFolder);}
    }

    public static void ClearTrash()
    {
        Task.Run(() =>
        {
            try
            {
                foreach (string file in Directory.GetFiles(TrashFolder))
                {
                    File.Delete(file);
                }
                foreach (string directory in Directory.GetDirectories(TrashFolder))
                {
                    Directory.Delete(directory, true);
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Error clearing trash folder", e);
            }
        });
    }
    
    public static void Save(string filePath, object data)
    {
        string directory = Path.GetDirectoryName(filePath) ?? string.Empty;
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }

    public static T? Load<T>(string filePath) where T : class
    {
        if (!File.Exists(filePath)) return null;
        string json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<T>(json);
    }
}