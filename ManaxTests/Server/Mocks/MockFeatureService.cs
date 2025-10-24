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

    public FeaturesDto GetEnabledFeatures()
    {
        
        return new FeaturesDto(Enum.GetValues(typeof(FeatureType)).Cast<FeatureType>().ToList());
    }

    public void SetFeatureEnabled(FeatureType featureType, bool enabled)
    {
    }

    public void SetFeatureEnabled(string featureName, bool enabled)
    {
    }
}