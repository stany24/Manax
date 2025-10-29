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
        return featureService.GetFeatures();
    }
    
    [HttpPost]
    [RequirePermission(Permission.WriteFeatures)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult SetFeature(Feature feature)
    {
        featureService.SetFeatureEnabled(feature);
        return Ok();
    }
    
    [HttpPut("/api/features")]
    [RequirePermission(Permission.WriteFeatures)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult SetFeatures(List<Feature> features)
    {
        foreach (Feature feature in features)
        {
            featureService.SetFeatureEnabled(feature);
        }
        return Ok();
    }
}