using ManaxLibrary.DTO.Feature;
using ManaxServer.Services.Feature;

namespace ManaxTests.Server.Mocks;

public class MockFeatureLoader : IFeatureLoader
{
    public List<Feature> Load()
    {
        return
        [
            new Feature
            {
                Key = FeatureType.Ranks,
                Value = false
            },

            new Feature
            {
                Key = FeatureType.AutomaticIssues,
                Value = false
            },

            new Feature
            {
                Key = FeatureType.ReportedIssues,
                Value = false
            }
        ];
    }
}