using ManaxLibrary.DTO.Feature;
using ManaxServer.Services.Feature;

namespace ManaxTests.Server.Mocks;

public class MockFeatureSaver:IFeatureSaver
{
    public void Save(List<Feature> features) { }
}