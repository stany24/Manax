using ManaxLibrary.DTO.Feature;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxLibrary.DTO.User;
using ManaxServer.Attributes;
using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Services.Feature;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/issue")]
[ApiController]
public class IssueTypeController(ManaxContext context, IFeatureService featureService): ControllerBase
{
    [HttpGet("chapter/reported/types")]
    [RequirePermission(Permission.ReadAllIssues)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IssueChapterReportedTypeDto>>> GetAllReportedChapterIssuesTypes()
    {
        if (!featureService.IsFeatureEnabled(FeatureType.ReportedIssues))
            return BadRequest(Localizer.FeatureDisabled(FeatureType.ReportedIssues));

        return await context.ReportedIssueChapterTypes.Select(i => i.ToDto())
            .ToListAsync();
    }
    
    [HttpGet("serie/reported/types")]
    [RequirePermission(Permission.ReadAllIssues)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IssueSerieReportedTypeDto>>> GetAllReportedSerieIssuesTypes()
    {
        if (!featureService.IsFeatureEnabled(FeatureType.ReportedIssues))
            return BadRequest(Localizer.FeatureDisabled(FeatureType.ReportedIssues));

        return await context.ReportedIssueSerieTypes.Select(i => i.ToDto())
            .ToListAsync();
    }
}