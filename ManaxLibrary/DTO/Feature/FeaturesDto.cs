namespace ManaxLibrary.DTO.Feature;

public class FeaturesDto
{
    public List<KeyValuePair<FeatureType,bool>> Features { get; set;  } = [];

    public FeaturesDto(List<FeatureType> enabledFeatures)
    {
        IEnumerable<FeatureType> values = Enum.GetValues(typeof(FeatureType)).Cast<FeatureType>();
        foreach (FeatureType value in values)
        {
            Features.Add(new KeyValuePair<FeatureType, bool>(value, enabledFeatures.Contains(value)));
        }
    }

    public bool IsEnabled(FeatureType feature)
    {
        return Features.FirstOrDefault(f => f.Key == feature).Value;
    }
}