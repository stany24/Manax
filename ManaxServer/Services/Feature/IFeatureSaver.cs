namespace ManaxServer.Services.Feature;

public interface IFeatureSaver
{
    public void Save(List<ManaxLibrary.DTO.Feature.Feature> features);
}