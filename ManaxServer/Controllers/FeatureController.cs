using ManaxLibrary.DTO.Feature;
using ManaxLibrary.DTO.User;
using ManaxServer.Attributes;
using ManaxServer.Services.Feature;
using Microsoft.AspNetCore.Mvc;

namespace ManaxServer.Controllers;

[Route("api/feature")]
[ApiController]
public class FeatureController(IFeatureService featureService)
    : ControllerBase
{
    [HttpGet("/api/features")]
    [RequirePermission(Permission.ReadFeatures)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public List<Feature> GetFeatures()
    {
        return featureService.GetEnabledFeatures();
    }
    
    [HttpPost("{featureName}/{enabled:bool}")]
    [RequirePermission(Permission.WriteFeatures)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult SetFeature(string featureName, bool enabled)
    {
        featureService.SetFeatureEnabled(featureName, enabled);
        return Ok();
    }
    
    [HttpPut("/api/features")]
    [RequirePermission(Permission.WriteFeatures)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult SetFeatures(FeaturesManager features)
    {
        foreach (Feature feature in features.Features)
        {
            featureService.SetFeatureEnabled(feature.Key, feature.Value);
        }
        return Ok();
    }
}