using ManaxLibrary;
using ManaxLibrary.DTO.Feature;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxLibrary.DTO.User;
using ManaxServer.Attributes;
using ManaxServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/issue")]
[ApiController]
public class IssueTypeController(ManaxContext context) : ControllerBase
{
    [HttpGet("chapter/reported/types")]
    [RequirePermission(Permission.ReadAllIssues)]
    [RequireFeature(FeatureType.ReportedIssues)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IssueChapterReportedTypeDto>>> GetAllReportedChapterIssuesTypes()
    {
        List<IssueChapterReportedTypeDto> types =  await context.ReportedIssueChapterTypes
            .Select(i => i.ToDto())
            .ToListAsync();
        return Ok(types);
    }

    [HttpGet("serie/reported/types")]
    [RequirePermission(Permission.ReadAllIssues)]
    [RequireFeature(FeatureType.ReportedIssues)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IssueSerieReportedTypeDto>>> GetAllReportedSerieIssuesTypes()
    {
        List<IssueSerieReportedTypeDto> types = await context.ReportedIssueSerieTypes
            .Select(i => i.ToDto())
            .ToListAsync();
        return Ok(types);
    }
}