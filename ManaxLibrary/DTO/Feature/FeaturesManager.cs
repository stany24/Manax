namespace ManaxLibrary.DTO.Feature;

public class FeaturesManager
{
    public List<Feature> Features { get; set; } = [];

    public FeaturesManager(List<FeatureType> enabledFeatures)
    {
        IEnumerable<FeatureType> values = Enum.GetValues(typeof(FeatureType)).Cast<FeatureType>();
        foreach (FeatureType value in values)
        {
            Features.Add(new Feature { Key = value, Value = enabledFeatures.Contains(value) });
        }
    }
    
    public FeaturesManager(List<Feature> features)
    {
        Features = features;
    }

    public bool IsEnabled(FeatureType feature)
    {
        return Features.FirstOrDefault(f => f.Key == feature)?.Value ?? false;
    }
}