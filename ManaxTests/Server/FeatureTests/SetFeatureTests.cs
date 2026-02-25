using ManaxLibrary.DTO.Feature;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.Server.FeatureTests;

[TestClass]
public class SetFeatureTests : FeatureTestsSetup
{
    [TestMethod]
    public void SetFeatureEnablesFeature()
    {
        ActionResult result = Controller.SetFeature(new Feature { Key = FeatureType.Ranks, Value = true });

        Assert.IsInstanceOfType<OkResult>(result);
        Assert.IsTrue(FeatureService.IsFeatureEnabled(FeatureType.Ranks));
    }

    [TestMethod]
    public void SetFeatureDisablesFeature()
    {
        FeatureService.SetFeatureEnabled(FeatureType.Ranks, true);

        ActionResult result = Controller.SetFeature(new Feature { Key = FeatureType.Ranks, Value = false });

        Assert.IsInstanceOfType<OkResult>(result);
        Assert.IsFalse(FeatureService.IsFeatureEnabled(FeatureType.Ranks));
    }

    [TestMethod]
    public void SetFeatureEnablingAlreadyEnabledFeatureReturnsOk()
    {
        FeatureService.SetFeatureEnabled(FeatureType.Ranks, true);

        ActionResult result = Controller.SetFeature(new Feature { Key = FeatureType.Ranks, Value = true });

        Assert.IsInstanceOfType<OkResult>(result);
        Assert.IsTrue(FeatureService.IsFeatureEnabled(FeatureType.Ranks));
    }

    [TestMethod]
    public void SetFeatureDisablingAlreadyDisabledFeatureReturnsOk()
    {
        ActionResult result = Controller.SetFeature(new Feature { Key = FeatureType.Ranks, Value = false });

        Assert.IsInstanceOfType<OkResult>(result);
        Assert.IsFalse(FeatureService.IsFeatureEnabled(FeatureType.Ranks));
    }

    [TestMethod]
    public void SetFeatureTogglesBetweenEnabledAndDisabled()
    {
        Controller.SetFeature(new Feature { Key = FeatureType.Ranks, Value = true });
        Assert.IsTrue(FeatureService.IsFeatureEnabled(FeatureType.Ranks));

        Controller.SetFeature(new Feature { Key = FeatureType.Ranks, Value = false });
        Assert.IsFalse(FeatureService.IsFeatureEnabled(FeatureType.Ranks));

        Controller.SetFeature(new Feature { Key = FeatureType.Ranks, Value = true });
        Assert.IsTrue(FeatureService.IsFeatureEnabled(FeatureType.Ranks));
    }

    [TestMethod]
    public void SetFeatureDoesNotAffectOtherFeatures()
    {
        FeatureService.SetFeatureEnabled(FeatureType.AutomaticIssues, true);

        Controller.SetFeature(new Feature { Key = FeatureType.Ranks, Value = true });

        Assert.IsTrue(FeatureService.IsFeatureEnabled(FeatureType.Ranks));
        Assert.IsTrue(FeatureService.IsFeatureEnabled(FeatureType.AutomaticIssues));
        Assert.IsFalse(FeatureService.IsFeatureEnabled(FeatureType.ReportedIssues));
    }

    [TestMethod]
    public void SetFeaturesWithEmptyListReturnsOk()
    {
        ActionResult result = Controller.SetFeatures([]);

        Assert.IsInstanceOfType<OkResult>(result);
    }

    [TestMethod]
    public void SetFeaturesWithSingleFeatureEnablesIt()
    {
        List<Feature> features = [new() { Key = FeatureType.Ranks, Value = true }];

        ActionResult result = Controller.SetFeatures(features);

        Assert.IsInstanceOfType<OkResult>(result);
        Assert.IsTrue(FeatureService.IsFeatureEnabled(FeatureType.Ranks));
    }

    [TestMethod]
    public void SetFeaturesWithMultipleFeaturesEnablesAll()
    {
        List<Feature> features =
        [
            new() { Key = FeatureType.Ranks, Value = true },
            new() { Key = FeatureType.AutomaticIssues, Value = true }
        ];

        ActionResult result = Controller.SetFeatures(features);

        Assert.IsInstanceOfType<OkResult>(result);
        Assert.IsTrue(FeatureService.IsFeatureEnabled(FeatureType.Ranks));
        Assert.IsTrue(FeatureService.IsFeatureEnabled(FeatureType.AutomaticIssues));
    }

    [TestMethod]
    public void SetFeaturesWithMixedEnabledAndDisabledFeaturesAppliesBoth()
    {
        FeatureService.SetFeatureEnabled(FeatureType.Ranks, true);
        FeatureService.SetFeatureEnabled(FeatureType.AutomaticIssues, true);
        List<Feature> features =
        [
            new() { Key = FeatureType.Ranks, Value = false },
            new() { Key = FeatureType.AutomaticIssues, Value = true },
            new() { Key = FeatureType.ReportedIssues, Value = true }
        ];

        ActionResult result = Controller.SetFeatures(features);

        Assert.IsInstanceOfType<OkResult>(result);
        Assert.IsFalse(FeatureService.IsFeatureEnabled(FeatureType.Ranks));
        Assert.IsTrue(FeatureService.IsFeatureEnabled(FeatureType.AutomaticIssues));
        Assert.IsTrue(FeatureService.IsFeatureEnabled(FeatureType.ReportedIssues));
    }

    [TestMethod]
    public void SetFeaturesWithAllFeaturesDisabledClearsAllFeatures()
    {
        FeatureService.SetFeatureEnabled(FeatureType.Ranks, true);
        FeatureService.SetFeatureEnabled(FeatureType.AutomaticIssues, true);
        FeatureService.SetFeatureEnabled(FeatureType.ReportedIssues, true);
        List<Feature> features =
        [
            new() { Key = FeatureType.Ranks, Value = false },
            new() { Key = FeatureType.AutomaticIssues, Value = false },
            new() { Key = FeatureType.ReportedIssues, Value = false }
        ];

        ActionResult result = Controller.SetFeatures(features);

        Assert.IsInstanceOfType<OkResult>(result);
        Assert.IsFalse(FeatureService.IsFeatureEnabled(FeatureType.Ranks));
        Assert.IsFalse(FeatureService.IsFeatureEnabled(FeatureType.AutomaticIssues));
        Assert.IsFalse(FeatureService.IsFeatureEnabled(FeatureType.ReportedIssues));
    }
}