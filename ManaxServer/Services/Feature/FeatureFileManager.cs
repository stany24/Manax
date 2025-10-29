using ManaxLibrary.Logging;

namespace ManaxServer.Services.Feature;

public class FeatureFileManager: IFeatureSaver, IFeatureLoader
{
    private const string FileName = "features.json";
    
    public void Save(List<ManaxLibrary.DTO.Feature.Feature> features)
    {        
        string filePath = Path.Combine(AppContext.BaseDirectory,FileName);
        string json = System.Text.Json.JsonSerializer.Serialize(features.ToList());
        try
        {
            File.Delete(filePath);
            File.WriteAllText(filePath, json);
        }
        catch(Exception e)
        {
            Logger.LogError("Failed to save the enabled features",e);
        }
    }

    public List<ManaxLibrary.DTO.Feature.Feature> Load()
    {
        string filePath = Path.Combine(AppContext.BaseDirectory,FileName);
        if (!File.Exists(filePath))
            return [];
        string json = File.ReadAllText(filePath);
        List<ManaxLibrary.DTO.Feature.Feature>? features = System.Text.Json.JsonSerializer.Deserialize<List<ManaxLibrary.DTO.Feature.Feature>>(json);
        return features ?? [];
    }
}