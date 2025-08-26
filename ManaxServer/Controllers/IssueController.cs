using ManaxLibrary.DTO.Issue.Automatic;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Models.Issue.Automatic;
using ManaxServer.Models.Issue.Reported;
using ManaxServer.Services.Mapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/issue")]
[ApiController]
public class IssueController(ManaxContext context, IMapper mapper) : ControllerBase
{
    [HttpGet("chapter/automatic")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AutomaticIssueChapterDto>>> GetAllAutomaticChapterIssues()
    {
        return await context.AutomaticIssuesChapter
            .Select(i => mapper.Map<AutomaticIssueChapterDto>(i))
            .ToListAsync();
    }

    [HttpGet("serie/automatic")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AutomaticIssueSerieDto>>> GetAllAutomaticSerieIssues()
    {
        return await context.AutomaticIssuesSerie
            .Select(i => mapper.Map<AutomaticIssueSerieDto>(i))
            .ToListAsync();
    }

    [HttpGet("chapter/reported")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReportedIssueChapterDto>>> GetAllReportedChapterIssues()
    {
        return await context.ReportedIssuesChapter
            .Select(i => mapper.Map<ReportedIssueChapterDto>(i))
            .ToListAsync();
    }

    [HttpGet("chapter/reported/types")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<List<ReportedIssueChapterTypeDto>> GetAllReportedChapterIssuesTypes()
    {
        return await context.ReportedIssueChapterTypes.Select(i => mapper.Map<ReportedIssueChapterTypeDto>(i))
            .ToListAsync();
    }

    [HttpGet("serie/reported")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReportedIssueSerieDto>>> GetAllReportedSerieIssues()
    {
        return await context.ReportedIssuesSerie
            .Select(i => mapper.Map<ReportedIssueSerieDto>(i))
            .ToListAsync();
    }

    [HttpGet("serie/reported/types")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReportedIssueSerieType>>> GetAllReportedSerieIssuesTypes()
    {
        List<ReportedIssueSerieType> issues = await context.ReportedIssueSerieTypes.ToListAsync();
        return mapper.Map<List<ReportedIssueSerieType>>(issues);
    }

    [HttpPost("chapter")]
    [Authorize(Roles = "User,Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> CreateChapterIssue(ReportedIssueChapterCreateDto reportedIssueChapterCreate)
    {
        long? currentUserId = UserController.GetCurrentUserId(HttpContext);
        if (currentUserId == null) return Unauthorized();

        ReportedIssueChapter reportedIssueChapter = mapper.Map<ReportedIssueChapter>(reportedIssueChapterCreate);
        reportedIssueChapter.UserId = (long)currentUserId;
        reportedIssueChapter.CreatedAt = DateTime.UtcNow;
        context.ReportedIssuesChapter.Add(reportedIssueChapter);
        await context.SaveChangesAsync();

        return Created();
    }

    [HttpPost("serie")]
    [Authorize(Roles = "User,Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> CreateSerieIssue(ReportedIssueSerieCreateDto reportedIssueSerieCreate)
    {
        ReportedIssueSerie issue = mapper.Map<ReportedIssueSerie>(reportedIssueSerieCreate);
        issue.CreatedAt = DateTime.UtcNow;

        context.ReportedIssuesSerie.Add(issue);
        await context.SaveChangesAsync();

        return Created();
    }

    [HttpPut("chapter/{id:long}/close")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseChapterIssue(long id)
    {
        ReportedIssueChapter? issue = await context.ReportedIssuesChapter.FindAsync(id);

        if (issue == null) return NotFound(Localizer.Format("IssueNotFound", id));

        context.ReportedIssuesChapter.Remove(issue);
        await context.SaveChangesAsync();

        return Ok();
    }

    [HttpPut("serie/{id:long}/close")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseSerieIssue(long id)
    {
        ReportedIssueSerie? issue = await context.ReportedIssuesSerie.FindAsync(id);

        if (issue == null) return NotFound(Localizer.Format("IssueNotFound", id));

        context.ReportedIssuesSerie.Remove(issue);
        await context.SaveChangesAsync();

        return Ok();
    }
}