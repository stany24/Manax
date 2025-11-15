using ManaxLibrary.DTO.Feature;
using ManaxServer.Services.Notification;

namespace ManaxServer.Services.Feature;

public class FeatureService
    : Service, IFeatureService
{
    private readonly Dictionary<FeatureType, bool> _features;
    private readonly INotificationService _notificationService;
    private readonly IFeatureSaver _saver;

    public FeatureService(IFeatureLoader loader, IFeatureSaver saver, INotificationService notificationService)
    {
        _features = loader.Load().ToDictionary(f => f.Key, f => f.Value);
        _saver = saver;
        _notificationService = notificationService;
    }

    public bool IsFeatureEnabled(FeatureType featureType)
    {
        return _features.TryGetValue(featureType, out bool enabled) && enabled;
    }

    public List<ManaxLibrary.DTO.Feature.Feature> GetFeatures()
    {
        return _features
            .Select(kv => new ManaxLibrary.DTO.Feature.Feature
            {
                Key = kv.Key,
                Value = kv.Value
            })
            .ToList();
    }

    public void SetFeatureEnabled(ManaxLibrary.DTO.Feature.Feature feature)
    {
        if (feature.Value)
        {
            if (_features.TryGetValue(feature.Key, out bool value) && value) return;
            _features[feature.Key] = true;
        }
        else
        {
            if (_features.TryGetValue(feature.Key, out bool value) && !value) return;
            _features[feature.Key] = false;
        }

        List<ManaxLibrary.DTO.Feature.Feature> features = _features.Select(kv => new ManaxLibrary.DTO.Feature.Feature
            {
                Key = kv.Key,
                Value = kv.Value
            })
            .ToList();
        _saver.Save(features);
        _notificationService.NotifyFeatureChanged(feature);
    }

    public void SetFeatureEnabled(FeatureType featureType, bool enabled)
    {
        SetFeatureEnabled(new ManaxLibrary.DTO.Feature.Feature
        {
            Key = featureType,
            Value = enabled
        });
    }
}