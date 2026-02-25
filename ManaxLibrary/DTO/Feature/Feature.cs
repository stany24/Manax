namespace ManaxLibrary.DTO.Feature;

public class Feature
{
    public FeatureType Key { get; init; }
    public bool Value { get; init; }

    public override int GetHashCode()
    {
        return Key.GetHashCode();
    }
}