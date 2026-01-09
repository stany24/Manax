using ManaxLibrary;
using ManaxLibrary.DTO.Feature;
using ManaxLibrary.DTO.Issue.Automatic;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxLibrary.DTO.User;
using ManaxServer.Attributes;
using ManaxServer.Models;
using ManaxServer.Models.Issue.Reported;
using ManaxServer.Services.Notification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/issue")]
[ApiController]
public class IssueController(
    ManaxContext context,
    INotificationService notificationService)
    : ControllerBase
{
    [HttpGet("chapter/automatic")]
    [RequirePermission(Permission.ReadAllIssues)]
    [RequireFeature(FeatureType.AutomaticIssues)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IssueChapterAutomaticDto>>> GetAllAutomaticChapterIssues()
    {
        List<IssueChapterAutomaticDto> issues = await context.AutomaticIssuesChapter
            .Select(i => i.ToDto())
            .ToListAsync();
        return Ok(issues);
    }

    [HttpGet("serie/automatic")]
    [RequirePermission(Permission.ReadAllIssues)]
    [RequireFeature(FeatureType.AutomaticIssues)]

    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IssueSerieAutomaticDto>>> GetAllAutomaticSerieIssues()
    {
        List<IssueSerieAutomaticDto> issues = await context.AutomaticIssuesSerie
            .Select(i => i.ToDto())
            .ToListAsync();
        return Ok(issues);
    }

    [HttpGet("chapter/reported")]
    [RequirePermission(Permission.ReadAllIssues)]
    [RequireFeature(FeatureType.ReportedIssues)]

    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IssueChapterReportedDto>>> GetAllReportedChapterIssues()
    {
        List<IssueChapterReportedDto> issues = await context.ReportedIssuesChapter
            .Select(i => i.ToDto())
            .ToListAsync();
        return Ok(issues);
    }

    [HttpGet("serie/reported")]
    [RequirePermission(Permission.ReadAllIssues)]
    [RequireFeature(FeatureType.ReportedIssues)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IssueSerieReportedDto>>> GetAllReportedSerieIssues()
    {
        List<IssueSerieReportedDto> issues = await context.ReportedIssuesSerie
            .Select(i => i.ToDto())
            .ToListAsync();
        return Ok(issues);
    }

    [HttpPost("chapter")]
    [RequirePermission(Permission.WriteIssues)]
    [RequireFeature(FeatureType.ReportedIssues)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> CreateChapterIssue(IssueChapterReportedCreateDto issueChapterReportedCreate)
    {
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
    [RequireFeature(FeatureType.ReportedIssues)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> CreateSerieIssue(IssueSerieReportedCreateDto issueSerieReportedCreate)
    {
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
    [RequireFeature(FeatureType.ReportedIssues)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CloseChapterIssue(long id)
    {
        IssueChapterReported? issue = await context.ReportedIssuesChapter.FindAsync(id);

        if (issue == null) return NotFound(ErrorCode.IssueDoesNotExist);

        context.ReportedIssuesChapter.Remove(issue);
        await context.SaveChangesAsync();
        notificationService.NotifyChapterIssueDeletedAsync(issue.Id);

        return Ok();
    }

    [HttpPut("serie/{id:long}/close")]
    [RequirePermission(Permission.DeleteIssues)]
    [RequireFeature(FeatureType.ReportedIssues)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CloseSerieIssue(long id)
    {
        IssueSerieReported? issue = await context.ReportedIssuesSerie.FindAsync(id);

        if (issue == null) return NotFound(ErrorCode.IssueDoesNotExist);

        context.ReportedIssuesSerie.Remove(issue);
        await context.SaveChangesAsync();
        notificationService.NotifySerieIssueDeletedAsync(issue.Id);

        return Ok();
    }
}