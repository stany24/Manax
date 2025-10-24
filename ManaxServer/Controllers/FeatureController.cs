using ManaxLibrary.DTO.Feature;
using ManaxLibrary.DTO.User;
using ManaxServer.Attributes;
using ManaxServer.Services.Feature;
using Microsoft.AspNetCore.Mvc;

namespace ManaxServer.Controllers;

[Route("api/tag")]
[ApiController]
public class FeatureController(IFeatureService featureService)
    : ControllerBase
{
    [HttpGet("/api/features")]
    [RequirePermission(Permission.ReadFeatures)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IEnumerable<FeatureType> GetFeatures()
    {
        return featureService.GetEnabledFeatures();
    }
    
    [HttpPost]
    [RequirePermission(Permission.WriteFeatures)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult SetFeature(string featureName, bool enabled)
    {
        featureService.SetFeatureEnabled(featureName, enabled);
        return Ok();
    }
    
    [HttpPost]
    [RequirePermission(Permission.WriteFeatures)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult SetFeatures(FeaturesDto features)
    {
        foreach (KeyValuePair<FeatureType, bool> feature in features.Features)
        {
            featureService.SetFeatureEnabled(feature.Key, feature.Value);
        }
        return Ok();
    }
}