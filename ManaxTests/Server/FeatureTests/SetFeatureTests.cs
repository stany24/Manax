using ManaxLibrary.DTO.Feature;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.Server.FeatureTests;

[TestClass]
public class SetFeatureTests : FeatureTestsSetup
{
    [TestMethod]
    public void SetFeatureWithValidFeatureTypeEnablesFeature()
    {
        IActionResult result = Controller.SetFeature(nameof(FeatureType.Ranks), true);

        Assert.IsInstanceOfType(result, typeof(OkResult));
        Assert.IsTrue(FeatureService.IsFeatureEnabled(FeatureType.Ranks));
    }

    [TestMethod]
    public void SetFeatureWithValidFeatureTypeDisablesFeature()
    {
        FeatureService.SetFeatureEnabled(FeatureType.Ranks, true);

        IActionResult result = Controller.SetFeature(nameof(FeatureType.Ranks), false);

        Assert.IsInstanceOfType(result, typeof(OkResult));
        Assert.IsFalse(FeatureService.IsFeatureEnabled(FeatureType.Ranks));
    }

    [TestMethod]
    public void SetFeatureWithInvalidFeatureNameStillReturnsOk()
    {
        IActionResult result = Controller.SetFeature("InvalidFeatureName", true);

        Assert.IsInstanceOfType(result, typeof(OkResult));
    }

    [TestMethod]
    public void SetFeatureWithInvalidFeatureNameDoesNotEnableAnything()
    {
        Controller.SetFeature("InvalidFeatureName", true);

        List<Feature> enabledFeatures = Controller.GetFeatures();
        Assert.HasCount(0, enabledFeatures);
    }

    [TestMethod]
    public void SetFeatureEnablingAlreadyEnabledFeatureReturnsOk()
    {
        FeatureService.SetFeatureEnabled(FeatureType.Ranks, true);

        IActionResult result = Controller.SetFeature(nameof(FeatureType.Ranks), true);

        Assert.IsInstanceOfType(result, typeof(OkResult));
        Assert.IsTrue(FeatureService.IsFeatureEnabled(FeatureType.Ranks));
    }

    [TestMethod]
    public void SetFeatureDisablingAlreadyDisabledFeatureReturnsOk()
    {
        IActionResult result = Controller.SetFeature(nameof(FeatureType.Ranks), false);

        Assert.IsInstanceOfType(result, typeof(OkResult));
        Assert.IsFalse(FeatureService.IsFeatureEnabled(FeatureType.Ranks));
    }

    [TestMethod]
    public void SetFeatureTogglesBetweenEnabledAndDisabled()
    {
        Controller.SetFeature(nameof(FeatureType.Ranks), true);
        Assert.IsTrue(FeatureService.IsFeatureEnabled(FeatureType.Ranks));

        Controller.SetFeature(nameof(FeatureType.Ranks), false);
        Assert.IsFalse(FeatureService.IsFeatureEnabled(FeatureType.Ranks));

        Controller.SetFeature(nameof(FeatureType.Ranks), true);
        Assert.IsTrue(FeatureService.IsFeatureEnabled(FeatureType.Ranks));
    }

    [TestMethod]
    public void SetFeatureDoesNotAffectOtherFeatures()
    {
        FeatureService.SetFeatureEnabled(FeatureType.AutomaticIssues, true);

        Controller.SetFeature(nameof(FeatureType.Ranks), true);

        Assert.IsTrue(FeatureService.IsFeatureEnabled(FeatureType.Ranks));
        Assert.IsTrue(FeatureService.IsFeatureEnabled(FeatureType.AutomaticIssues));
        Assert.IsFalse(FeatureService.IsFeatureEnabled(FeatureType.ReportedIssues));
    }

    [TestMethod]
    public void SetFeatureWithCaseInsensitiveFeatureNameEnablesFeature()
    {
        IActionResult result = Controller.SetFeature("ranks", true);

        Assert.IsInstanceOfType(result, typeof(OkResult));
        Assert.IsTrue(FeatureService.IsFeatureEnabled(FeatureType.Ranks));
    }

    [TestMethod]
    public void SetFeaturesWithEmptyListReturnsOk()
    {
        IActionResult result = Controller.SetFeatures([]);

        Assert.IsInstanceOfType(result, typeof(OkResult));
    }

    [TestMethod]
    public void SetFeaturesWithSingleFeatureEnablesIt()
    {
        List<Feature> features = [new() { Key = FeatureType.Ranks, Value = true }];

        IActionResult result = Controller.SetFeatures(features);

        Assert.IsInstanceOfType(result, typeof(OkResult));
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

        IActionResult result = Controller.SetFeatures(features);

        Assert.IsInstanceOfType(result, typeof(OkResult));
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

        IActionResult result = Controller.SetFeatures(features);

        Assert.IsInstanceOfType(result, typeof(OkResult));
        Assert.IsFalse(FeatureService.IsFeatureEnabled(FeatureType.Ranks));
        Assert.IsTrue(FeatureService.IsFeatureEnabled(FeatureType.AutomaticIssues));
        Assert.IsTrue(FeatureService.IsFeatureEnabled(FeatureType.ReportedIssues));
    }

    [TestMethod]
    public void SetFeaturesWithAllFeaturesDisabledClearsAllFeatures()
    {
        FeatureService.SetFeatureEnabled(FeatureType.Ranks, true);
        FeatureService.SetFeatureEnabled(FeatureType.AutomaticIssues, true);

        List<Feature> features =
        [
            new() { Key = FeatureType.Ranks, Value = false },
            new() { Key = FeatureType.AutomaticIssues, Value = false }
        ];

        IActionResult result = Controller.SetFeatures(features);

        Assert.IsInstanceOfType(result, typeof(OkResult));
        Assert.IsFalse(FeatureService.IsFeatureEnabled(FeatureType.Ranks));
        Assert.IsFalse(FeatureService.IsFeatureEnabled(FeatureType.AutomaticIssues));
        List<Feature> enabledFeatures = Controller.GetFeatures();
        Assert.HasCount(0, enabledFeatures);
    }

    [TestMethod]
    public void SetFeaturesOverwritesPreviousState()
    {
        FeatureService.SetFeatureEnabled(FeatureType.Ranks, true);

        List<Feature> features = [new() { Key = FeatureType.AutomaticIssues, Value = true }];

        IActionResult result = Controller.SetFeatures(features);

        Assert.IsInstanceOfType(result, typeof(OkResult));
        Assert.IsTrue(FeatureService.IsFeatureEnabled(FeatureType.Ranks));
        Assert.IsTrue(FeatureService.IsFeatureEnabled(FeatureType.AutomaticIssues));
    }

    [TestMethod]
    public void SetFeaturesVerifyCountAfterMultipleChanges()
    {
        List<Feature> features =
        [
            new() { Key = FeatureType.Ranks, Value = true },
            new() { Key = FeatureType.AutomaticIssues, Value = true },
            new() { Key = FeatureType.ReportedIssues, Value = true }
        ];

        Controller.SetFeatures(features);

        List<Feature> enabledFeatures = Controller.GetFeatures();
        Assert.HasCount(3, enabledFeatures);
    }
}

