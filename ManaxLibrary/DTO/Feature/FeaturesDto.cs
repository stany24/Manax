using System.Collections.ObjectModel;

namespace ManaxLibrary.DTO.Feature;

public class FeaturesDto
{
    public ObservableCollection<KeyValuePair<FeatureType,bool>> Features { get; set;  } = [];

    public FeaturesDto(List<FeatureType> enabledFeatures)
    {
        IEnumerable<FeatureType> values = Enum.GetValues(typeof(FeatureType)).Cast<FeatureType>();
        foreach (FeatureType value in values)
        {
            Features.Add(new KeyValuePair<FeatureType, bool>(value, enabledFeatures.Contains(value)));
        }
    }
}