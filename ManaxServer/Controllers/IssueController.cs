using ManaxLibrary;
using ManaxLibrary.DTO.Feature;
using ManaxLibrary.DTO.Issue.Automatic;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxLibrary.DTO.User;
using ManaxServer.Attributes;
using ManaxServer.Models;
using ManaxServer.Models.Issue.Reported;
using ManaxServer.Services.Feature;
using ManaxServer.Services.Notification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/issue")]
[ApiController]
public class IssueController(
    ManaxContext context,
    INotificationService notificationService,
    IFeatureService featureService)
    : ControllerBase
{
    [HttpGet("chapter/automatic")]
    [RequirePermission(Permission.ReadAllIssues)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IssueChapterAutomaticDto>>> GetAllAutomaticChapterIssues()
    {
        if (!featureService.IsFeatureEnabled(FeatureType.AutomaticIssues))
            return BadRequest(ErrorCode.FeatureDisabled);

        return await context.AutomaticIssuesChapter
            .Select(i => i.ToDto())
            .ToListAsync();
    }

    [HttpGet("serie/automatic")]
    [RequirePermission(Permission.ReadAllIssues)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IssueSerieAutomaticDto>>> GetAllAutomaticSerieIssues()
    {
        if (!featureService.IsFeatureEnabled(FeatureType.AutomaticIssues))
            return BadRequest(ErrorCode.FeatureDisabled);

        return await context.AutomaticIssuesSerie
            .Select(i => i.ToDto())
            .ToListAsync();
    }

    [HttpGet("chapter/reported")]
    [RequirePermission(Permission.ReadAllIssues)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IssueChapterReportedDto>>> GetAllReportedChapterIssues()
    {
        if (!featureService.IsFeatureEnabled(FeatureType.ReportedIssues))
            return BadRequest(ErrorCode.FeatureDisabled);

        return await context.ReportedIssuesChapter
            .Select(i => i.ToDto())
            .ToListAsync();
    }

    [HttpGet("serie/reported")]
    [RequirePermission(Permission.ReadAllIssues)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IssueSerieReportedDto>>> GetAllReportedSerieIssues()
    {
        if (!featureService.IsFeatureEnabled(FeatureType.ReportedIssues))
            return BadRequest(ErrorCode.FeatureDisabled);

        return await context.ReportedIssuesSerie
            .Select(i => i.ToDto())
            .ToListAsync();
    }

    [HttpPost("chapter")]
    [RequirePermission(Permission.WriteIssues)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> CreateChapterIssue(IssueChapterReportedCreateDto issueChapterReportedCreate)
    {
        if (!featureService.IsFeatureEnabled(FeatureType.ReportedIssues))
            return BadRequest(ErrorCode.FeatureDisabled);

        long? currentUserId = UserController.GetCurrentUserId(HttpContext);
        if (currentUserId == null) return Unauthorized(ErrorCode.TokenRequired);

        IssueChapterReported issue = IssueChapterReported.Create(issueChapterReportedCreate, (long)currentUserId);
        issue.UserId = (long)currentUserId;
        issue.CreatedAt = DateTime.UtcNow;

        context.ReportedIssuesChapter.Add(issue);
        try { await context.SaveChangesAsync(); }
        catch { return Conflict(ErrorCode.IssueAlreadyExists); }
        notificationService.NotifyChapterIssueCreatedAsync(issue.ToDto());

        return Created();
    }

    [HttpPost("serie")]
    [RequirePermission(Permission.WriteIssues)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> CreateSerieIssue(IssueSerieReportedCreateDto issueSerieReportedCreate)
    {
        if (!featureService.IsFeatureEnabled(FeatureType.ReportedIssues))
            return BadRequest(ErrorCode.FeatureDisabled);

        long? currentUserId = UserController.GetCurrentUserId(HttpContext);
        if (currentUserId == null) return Unauthorized(ErrorCode.TokenRequired);

        IssueSerieReported issue = IssueSerieReported.Create(issueSerieReportedCreate, (long)currentUserId);

        context.ReportedIssuesSerie.Add(issue);
        try { await context.SaveChangesAsync(); }
        catch { return Conflict(ErrorCode.IssueAlreadyExists); }
        notificationService.NotifySerieIssueCreatedAsync(issue.ToDto());

        return Created();
    }

    [HttpPut("chapter/{id:long}/close")]
    [RequirePermission(Permission.DeleteIssues)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseChapterIssue(long id)
    {
        if (!featureService.IsFeatureEnabled(FeatureType.ReportedIssues))
            return BadRequest(ErrorCode.FeatureDisabled);

        IssueChapterReported? issue = await context.ReportedIssuesChapter.FindAsync(id);

        if (issue == null) return NotFound(ErrorCode.IssueDoesNotExist);

        context.ReportedIssuesChapter.Remove(issue);
        await context.SaveChangesAsync();
        notificationService.NotifyChapterIssueDeletedAsync(issue.Id);

        return Ok();
    }

    [HttpPut("serie/{id:long}/close")]
    [RequirePermission(Permission.DeleteIssues)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseSerieIssue(long id)
    {
        if (!featureService.IsFeatureEnabled(FeatureType.ReportedIssues))
            return BadRequest(ErrorCode.FeatureDisabled);

        IssueSerieReported? issue = await context.ReportedIssuesSerie.FindAsync(id);

        if (issue == null) return NotFound(ErrorCode.IssueDoesNotExist);

        context.ReportedIssuesSerie.Remove(issue);
        await context.SaveChangesAsync();
        notificationService.NotifySerieIssueDeletedAsync(issue.Id);

        return Ok();
    }
}