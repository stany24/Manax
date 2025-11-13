using ManaxLibrary.DTO.Feature;
using ManaxLibrary.DTO.Issue.Automatic;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxLibrary.DTO.User;
using ManaxServer.Attributes;
using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Models.Issue.Reported;
using ManaxServer.Services.Feature;
using ManaxServer.Services.Notification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/issue")]
[ApiController]
public class IssueController(ManaxContext context, INotificationService notificationService, IFeatureService featureService)
    : ControllerBase
{
    [HttpGet("chapter/automatic")]
    [RequirePermission(Permission.ReadAllIssues)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IssueChapterAutomaticDto>>> GetAllAutomaticChapterIssues()
    {
        if (!featureService.IsFeatureEnabled(FeatureType.AutomaticIssues)) { return BadRequest(Localizer.FeatureDisabled(FeatureType.AutomaticIssues)); }

        return await context.AutomaticIssuesChapter
            .Select(i => i.ToDto())
            .ToListAsync();
    }

    [HttpGet("serie/automatic")]
    [RequirePermission(Permission.ReadAllIssues)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IssueSerieAutomaticDto>>> GetAllAutomaticSerieIssues()
    {
        if (!featureService.IsFeatureEnabled(FeatureType.AutomaticIssues)) { return BadRequest(Localizer.FeatureDisabled(FeatureType.AutomaticIssues)); }

        return await context.AutomaticIssuesSerie
            .Select(i => i.ToDto())
            .ToListAsync();
    }

    [HttpGet("chapter/reported")]
    [RequirePermission(Permission.ReadAllIssues)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IssueChapterReportedDto>>> GetAllReportedChapterIssues()
    {
        if (!featureService.IsFeatureEnabled(FeatureType.ReportedIssues)) { return BadRequest(Localizer.FeatureDisabled(FeatureType.ReportedIssues)); }
        
        return await context.ReportedIssuesChapter
            .Select(i => i.ToDto())
            .ToListAsync();
    }

    [HttpGet("chapter/reported/types")]
    [RequirePermission(Permission.ReadAllIssues)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IssueChapterReportedTypeDto>>> GetAllReportedChapterIssuesTypes()
    {
        if (!featureService.IsFeatureEnabled(FeatureType.ReportedIssues)) { return BadRequest(Localizer.FeatureDisabled(FeatureType.ReportedIssues)); }

        return await context.ReportedIssueChapterTypes.Select(i => i.ToDto())
            .ToListAsync();
    }

    [HttpGet("serie/reported")]
    [RequirePermission(Permission.ReadAllIssues)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IssueSerieReportedDto>>> GetAllReportedSerieIssues()
    {
        if (!featureService.IsFeatureEnabled(FeatureType.ReportedIssues)) { return BadRequest(Localizer.FeatureDisabled(FeatureType.ReportedIssues)); }

        return await context.ReportedIssuesSerie
            .Select(i => i.ToDto())
            .ToListAsync();
    }

    [HttpGet("serie/reported/types")]
    [RequirePermission(Permission.ReadAllIssues)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IssueSerieReportedTypeDto>>> GetAllReportedSerieIssuesTypes()
    {
        if (!featureService.IsFeatureEnabled(FeatureType.ReportedIssues)) { return BadRequest(Localizer.FeatureDisabled(FeatureType.ReportedIssues)); }

        return await context.ReportedIssueSerieTypes.Select(i => i.ToDto())
            .ToListAsync();
    }

    [HttpPost("chapter")]
    [RequirePermission(Permission.WriteIssues)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> CreateChapterIssue(IssueChapterReportedCreateDto issueChapterReportedCreate)
    {
        if (!featureService.IsFeatureEnabled(FeatureType.ReportedIssues)) { return BadRequest(Localizer.FeatureDisabled(FeatureType.ReportedIssues)); }

        long? currentUserId = UserController.GetCurrentUserId(HttpContext);
        if (currentUserId == null) return Unauthorized();

        bool issueExists = await context.ReportedIssuesChapter
            .AnyAsync(i => i.UserId == currentUserId &&
                           i.ChapterId == issueChapterReportedCreate.ChapterId &&
                           i.ProblemId == issueChapterReportedCreate.ProblemId);

        if (issueExists) return Conflict("Issue already reported for this chapter and problem type.");

        IssueChapterReported issue = IssueChapterReported.Create(issueChapterReportedCreate,(long)currentUserId);
        issue.UserId = (long)currentUserId;
        issue.CreatedAt = DateTime.UtcNow;

        context.ReportedIssuesChapter.Add(issue);
        await context.SaveChangesAsync();
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
        if (!featureService.IsFeatureEnabled(FeatureType.ReportedIssues)) { return BadRequest(Localizer.FeatureDisabled(FeatureType.ReportedIssues)); }

        long? currentUserId = UserController.GetCurrentUserId(HttpContext);
        if (currentUserId == null) return Unauthorized();

        bool issueExists = await context.ReportedIssuesSerie
            .AnyAsync(i => i.UserId == currentUserId &&
                           i.SerieId == issueSerieReportedCreate.SerieId &&
                           i.ProblemId == issueSerieReportedCreate.ProblemId);

        if (issueExists) return Conflict("Issue already reported for this series and problem type.");

        IssueSerieReported issue = IssueSerieReported.Create(issueSerieReportedCreate,(long)currentUserId);

        context.ReportedIssuesSerie.Add(issue);
        await context.SaveChangesAsync();
        notificationService.NotifySerieIssueCreatedAsync(issue.ToDto());

        return Created();
    }

    [HttpPut("chapter/{id:long}/close")]
    [RequirePermission(Permission.DeleteIssues)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseChapterIssue(long id)
    {
        if (!featureService.IsFeatureEnabled(FeatureType.ReportedIssues)) { return BadRequest(Localizer.FeatureDisabled(FeatureType.ReportedIssues)); }

        IssueChapterReported? issue = await context.ReportedIssuesChapter.FindAsync(id);

        if (issue == null) return NotFound(Localizer.IssueNotFound(id));

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
        if (!featureService.IsFeatureEnabled(FeatureType.ReportedIssues)) { return BadRequest(Localizer.FeatureDisabled(FeatureType.ReportedIssues)); }

        IssueSerieReported? issue = await context.ReportedIssuesSerie.FindAsync(id);

        if (issue == null) return NotFound(Localizer.IssueNotFound(id));

        context.ReportedIssuesSerie.Remove(issue);
        await context.SaveChangesAsync();
        notificationService.NotifySerieIssueDeletedAsync(issue.Id);

        return Ok();
    }
}