using ManaxLibrary.DTO.Feature;

namespace ManaxServer.Services.Feature;

public interface IFeatureService
{
    public bool IsFeatureEnabled(FeatureType featureType);
    public List<ManaxLibrary.DTO.Feature.Feature> GetFeatures();
    public void SetFeatureEnabled(ManaxLibrary.DTO.Feature.Feature feature);
    public void SetFeatureEnabled(FeatureType featureType, bool enabled);
}