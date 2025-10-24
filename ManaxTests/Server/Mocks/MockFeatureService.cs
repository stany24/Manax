using ManaxLibrary.DTO.Feature;
using ManaxServer.Services.Feature;

namespace ManaxTests.Server.Mocks;

public class MockFeatureService:IFeatureService
{
    public bool IsFeatureEnabled(FeatureType featureType)
    {
        return true;
    }

    public bool IsFeatureEnabled(string featureName)
    {
        return true;
    }

    public List<FeatureType> GetEnabledFeatures()
    {
        return (List<FeatureType>)Enum.GetValues(typeof(FeatureType)).Cast<FeatureType>();
    }

    public void SetFeatureEnabled(FeatureType featureType, bool enabled)
    {
    }

    public void SetFeatureEnabled(string featureName, bool enabled)
    {
    }
}