namespace ManaxLibrary.DTO.Feature;

public class FeaturesManager(List<Feature> features)
{
    public List<Feature> Features { get; } = features;

    public bool IsEnabled(FeatureType feature)
    {
        return Features.FirstOrDefault(f => f.Key == feature)?.Value ?? false;
    }
}