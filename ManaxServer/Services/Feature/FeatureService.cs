using ManaxLibrary.Logging;
using ManaxServer.Services.Notification;

namespace ManaxServer.Services.Feature;

public class FeatureService:Service,IFeatureService
{
    private readonly HashSet<FeatureType> _enabledFeatures = [];
    public readonly string FileName = "features.json";
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

    public List<FeatureType> GetEnabledFeatures()
    {
        return _enabledFeatures.ToList();
    }

    public void SetFeatureEnabled(FeatureType featureType, bool enabled)
    {
        if (enabled)
            _enabledFeatures.Add(featureType);
        else
            _enabledFeatures.Remove(featureType);
        Save();
        _notificationService.NotifyFeatureChanged(featureType,enabled);
    }

    public void SetFeatureEnabled(string featureName, bool enabled)
    {
        if (Enum.TryParse(featureName, out FeatureType featureType))
            SetFeatureEnabled(featureType, enabled);
        Save();
        _notificationService.NotifyFeatureChanged(featureType,enabled);
    }
}