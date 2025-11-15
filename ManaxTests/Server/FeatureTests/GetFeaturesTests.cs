using ManaxLibrary.DTO.Feature;

namespace ManaxTests.Server.FeatureTests;

[TestClass]
public class GetFeaturesTests : FeatureTestsSetup
{
    [TestMethod]
    public void GetFeaturesReturnsAllFeatures()
    {
        List<Feature> result = Controller.GetFeatures();

        Assert.IsNotNull(result);
        Assert.HasCount(3, result);
    }

    [TestMethod]
    public void GetFeaturesVerifyAllReturnedFeaturesAreEnabled()
    {
        FeatureService.SetFeatureEnabled(FeatureType.Ranks, true);
        FeatureService.SetFeatureEnabled(FeatureType.AutomaticIssues, true);
        FeatureService.SetFeatureEnabled(FeatureType.ReportedIssues, true);

        List<Feature> result = Controller.GetFeatures();

        Assert.IsNotNull(result);
        Assert.HasCount(3, result);
        foreach (Feature feature in result) Assert.IsTrue(feature.Value);
    }

    [TestMethod]
    public void GetFeaturesVerifyCorrectlySet()
    {
        FeatureService.SetFeatureEnabled(FeatureType.Ranks, true);

        List<Feature> result = Controller.GetFeatures();

        Assert.IsNotNull(result);
        Assert.IsTrue(FeatureService.IsFeatureEnabled(FeatureType.Ranks));
    }
}