using ManaxLibrary.DTO.Feature;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.Server.FeatureTests;

[TestClass]
public class GetFeaturesTests : FeatureTestsSetup
{
    [TestMethod]
    public void GetFeaturesReturnsAllFeatures()
    {
        ActionResult<List<Feature>> result = Controller.GetFeatures();
        
        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);

        List<Feature>? features = okResult.Value as List<Feature>;
        
        Assert.IsNotNull(features);
        Assert.HasCount(3, features);
    }

    [TestMethod]
    public void GetFeaturesVerifyAllReturnedFeaturesAreEnabled()
    {
        FeatureService.SetFeatureEnabled(FeatureType.Ranks, true);
        FeatureService.SetFeatureEnabled(FeatureType.AutomaticIssues, true);
        FeatureService.SetFeatureEnabled(FeatureType.ReportedIssues, true);

        ActionResult<List<Feature>> result = Controller.GetFeatures();

        OkObjectResult ? okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        List<Feature>? features = okResult.Value as List<Feature>;
        Assert.IsNotNull(features);
        Assert.HasCount(3, features);
        foreach (Feature feature in features) Assert.IsTrue(feature.Value);
    }

    [TestMethod]
    public void GetFeaturesVerifyCorrectlySet()
    {
        FeatureService.SetFeatureEnabled(FeatureType.Ranks, true);

        ActionResult<List<Feature>> result = Controller.GetFeatures();
        
        OkObjectResult ? okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        
        Assert.IsTrue(FeatureService.IsFeatureEnabled(FeatureType.Ranks));
    }
}