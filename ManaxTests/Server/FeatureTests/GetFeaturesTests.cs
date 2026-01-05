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
        Assert.IsNull(okResult);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Value);
        Assert.HasCount(3, result.Value);
    }

    [TestMethod]
    public void GetFeaturesVerifyAllReturnedFeaturesAreEnabled()
    {
        FeatureService.SetFeatureEnabled(FeatureType.Ranks, true);
        FeatureService.SetFeatureEnabled(FeatureType.AutomaticIssues, true);
        FeatureService.SetFeatureEnabled(FeatureType.ReportedIssues, true);

        ActionResult<List<Feature>> result = Controller.GetFeatures();

        OkObjectResult ? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);
        
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Value);
        Assert.HasCount(3, result.Value);
        foreach (Feature feature in result.Value) Assert.IsTrue(feature.Value);
    }

    [TestMethod]
    public void GetFeaturesVerifyCorrectlySet()
    {
        FeatureService.SetFeatureEnabled(FeatureType.Ranks, true);

        ActionResult<List<Feature>> result = Controller.GetFeatures();
        
        OkObjectResult ? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        Assert.IsNotNull(result);
        Assert.IsTrue(FeatureService.IsFeatureEnabled(FeatureType.Ranks));
    }
}