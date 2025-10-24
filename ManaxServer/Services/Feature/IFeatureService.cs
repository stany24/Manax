namespace ManaxServer.Services.Feature;

public interface IFeatureService
{
    public bool IsFeatureEnabled(FeatureType featureType);
    public bool IsFeatureEnabled(string featureName);
    public List<FeatureType> GetEnabledFeatures();
    public void SetFeatureEnabled(FeatureType featureType, bool enabled);
    public void SetFeatureEnabled(string featureName, bool enabled);
}