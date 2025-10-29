namespace ManaxServer.Services.Feature;

public interface IFeatureLoader
{
    public List<ManaxLibrary.DTO.Feature.Feature> Load();
}