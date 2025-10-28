using ManaxLibrary.DTO.Feature;

namespace ManaxTests.Server.FeatureTests;

[TestClass]
public class GetFeaturesTests : FeatureTestsSetup
{
    [TestMethod]
    public void GetFeaturesReturnsEmptyListWhenNoFeaturesEnabled()
    {
        List<Feature> result = Controller.GetFeatures();

        Assert.IsNotNull(result);
        Assert.HasCount(0, result);
    }

    [TestMethod]
    public void GetFeaturesReturnsAllEnabledFeatures()
    {
        FeatureService.SetFeatureEnabled(FeatureType.Ranks, true);
        FeatureService.SetFeatureEnabled(FeatureType.AutomaticIssues, true);

        List<Feature> result = Controller.GetFeatures();

        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
        Assert.Contains(result.First(f => f.Key == FeatureType.Ranks), result);
        Assert.Contains(result.First(f => f.Key == FeatureType.AutomaticIssues), result);
    }

    [TestMethod]
    public void GetFeaturesOnlyReturnsEnabledFeatures()
    {
        FeatureService.SetFeatureEnabled(FeatureType.Ranks, true);
        FeatureService.SetFeatureEnabled(FeatureType.AutomaticIssues, false);

        List<Feature> result = Controller.GetFeatures();

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Assert.AreEqual(FeatureType.Ranks, result[0].Key);
        Assert.IsTrue(result[0].Value);
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
        foreach (Feature feature in result)
        {
            Assert.IsTrue(feature.Value);
        }
    }

    [TestMethod]
    public void GetFeaturesVerifyCorrectKeyValuePairs()
    {
        FeatureService.SetFeatureEnabled(FeatureType.Ranks, true);

        List<Feature> result = Controller.GetFeatures();

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Feature feature = result[0];
        Assert.AreEqual(FeatureType.Ranks, feature.Key);
        Assert.IsTrue(feature.Value);
    }

    [TestMethod]
    public void GetFeaturesMultipleCallsReturnConsistentResults()
    {
        FeatureService.SetFeatureEnabled(FeatureType.Ranks, true);
        FeatureService.SetFeatureEnabled(FeatureType.AutomaticIssues, true);

        List<Feature> result1 = Controller.GetFeatures();
        List<Feature> result2 = Controller.GetFeatures();

        Assert.HasCount(2, result1);
        Assert.HasCount(2, result2);
    }
}

