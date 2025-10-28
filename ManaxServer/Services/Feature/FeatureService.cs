using ManaxLibrary.DTO.Feature;
using ManaxLibrary.Logging;
using ManaxServer.Services.Notification;

namespace ManaxServer.Services.Feature;

public class FeatureService:Service,IFeatureService
{
    private readonly HashSet<FeatureType> _enabledFeatures = [];
    private const string FileName = "features.json";
    private readonly INotificationService _notificationService;
    
    public FeatureService(INotificationService notificationService)
    {
        _notificationService = notificationService;
        Load();
    }

    private void Load()
    {
        string filePath = Path.Combine(AppContext.BaseDirectory,FileName);
        if (!File.Exists(filePath))
            return;
        string json = File.ReadAllText(filePath);
        List<FeatureType>? features = System.Text.Json.JsonSerializer.Deserialize<List<FeatureType>>(json);
        if (features != null)
            _enabledFeatures.UnionWith(features);
    }

    private void Save()
    {
        string filePath = Path.Combine(AppContext.BaseDirectory,FileName);
        string json = System.Text.Json.JsonSerializer.Serialize(_enabledFeatures.ToList());
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
    
    public bool IsFeatureEnabled(FeatureType featureType)
    {
        return _enabledFeatures.Contains(featureType);
    }

    public bool IsFeatureEnabled(string featureName)
    {
        return Enum.TryParse(featureName, out FeatureType featureType) && IsFeatureEnabled(featureType);
    }

    public List<ManaxLibrary.DTO.Feature.Feature> GetEnabledFeatures()
    {
        return _enabledFeatures.Select(f => new ManaxLibrary.DTO.Feature.Feature { Key = f, Value = true }).ToList();
    }

    public void SetFeatureEnabled(FeatureType featureType, bool enabled)
    {
        if (enabled) {if (!_enabledFeatures.Add(featureType)) { return; } }
        else { if (!_enabledFeatures.Remove(featureType)) { return; } }

        Save();
        _notificationService.NotifyFeatureChanged(new ManaxLibrary.DTO.Feature.Feature {Key = featureType, Value = enabled});
    }

    public void SetFeatureEnabled(string featureName, bool enabled)
    {
        if (Enum.TryParse(featureName, out FeatureType featureType))
            SetFeatureEnabled(featureType, enabled);
    }
}