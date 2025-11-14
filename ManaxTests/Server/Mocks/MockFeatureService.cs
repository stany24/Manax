using ManaxLibrary.DTO.Feature;
using ManaxServer.Services.Feature;

namespace ManaxTests.Server.Mocks;

public class MockFeatureService:IFeatureService
{
    private readonly HashSet<FeatureType> _enabledFeatures = [];
    
    public bool IsFeatureEnabled(FeatureType featureType)
    {
        return _enabledFeatures.Contains(featureType);
    }

    public List<Feature> GetFeatures()
    {
        return _enabledFeatures.Select(f => new Feature { Key = f, Value = true }).ToList();
    }

    public void SetFeatureEnabled(Feature feature)
    {
        SetFeatureEnabled(feature.Key, feature.Value);
    }

    public void SetFeatureEnabled(FeatureType featureType, bool enabled)
    {
        if (enabled)
            _enabledFeatures.Add(featureType);
        else
            _enabledFeatures.Remove(featureType);
    }
}