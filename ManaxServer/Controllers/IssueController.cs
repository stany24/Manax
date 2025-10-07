using ManaxLibrary.DTO.Issue.Automatic;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxLibrary.DTO.User;
using ManaxServer.Attributes;
using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Models.Issue.Reported;
using ManaxServer.Services.Mapper;
using ManaxServer.Services.Notification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/issue")]
[ApiController]
public class IssueController(ManaxContext context, IMapper mapper, INotificationService notificationService)
    : ControllerBase
{
    [HttpGet("chapter/automatic")]
    [RequirePermission(Permission.ReadAllIssues)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IssueChapterAutomaticDto>>> GetAllAutomaticChapterIssues()
    {
        return await context.AutomaticIssuesChapter
            .Select(i => mapper.Map<IssueChapterAutomaticDto>(i))
            .ToListAsync();
    }

    [HttpGet("serie/automatic")]
    [RequirePermission(Permission.ReadAllIssues)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IssueSerieAutomaticDto>>> GetAllAutomaticSerieIssues()
    {
        return await context.AutomaticIssuesSerie
            .Select(i => mapper.Map<IssueSerieAutomaticDto>(i))
            .ToListAsync();
    }

    [HttpGet("chapter/reported")]
    [RequirePermission(Permission.ReadAllIssues)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IssueChapterReportedDto>>> GetAllReportedChapterIssues()
    {
        return await context.ReportedIssuesChapter
            .Select(i => mapper.Map<IssueChapterReportedDto>(i))
            .ToListAsync();
    }

    [HttpGet("chapter/reported/types")]
    [RequirePermission(Permission.ReadAllIssues)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<List<IssueChapterReportedTypeDto>> GetAllReportedChapterIssuesTypes()
    {
        return await context.ReportedIssueChapterTypes.Select(i => mapper.Map<IssueChapterReportedTypeDto>(i))
            .ToListAsync();
    }

    [HttpGet("serie/reported")]
    [RequirePermission(Permission.ReadAllIssues)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IssueSerieReportedDto>>> GetAllReportedSerieIssues()
    {
        return await context.ReportedIssuesSerie
            .Select(i => mapper.Map<IssueSerieReportedDto>(i))
            .ToListAsync();
    }

    [HttpGet("serie/reported/types")]
    [RequirePermission(Permission.ReadAllIssues)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IssueSerieReportedTypeDto>>> GetAllReportedSerieIssuesTypes()
    {
        return await context.ReportedIssueSerieTypes.Select(i => mapper.Map<IssueSerieReportedTypeDto>(i))
            .ToListAsync();
    }

    [HttpPost("chapter")]
    [RequirePermission(Permission.WriteIssues)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> CreateChapterIssue(IssueChapterReportedCreateDto issueChapterReportedCreate)
    {
        long? currentUserId = UserController.GetCurrentUserId(HttpContext);
        if (currentUserId == null) return Unauthorized();

        bool issueExists = await context.ReportedIssuesChapter
            .AnyAsync(i => i.UserId == currentUserId &&
                           i.ChapterId == issueChapterReportedCreate.ChapterId &&
                           i.ProblemId == issueChapterReportedCreate.ProblemId);

        if (issueExists) return Conflict("Issue already reported for this chapter and problem type.");

        IssueChapterReported issue = mapper.Map<IssueChapterReported>(issueChapterReportedCreate);
        issue.UserId = (long)currentUserId;
        issue.CreatedAt = DateTime.UtcNow;

        context.ReportedIssuesChapter.Add(issue);
        await context.SaveChangesAsync();
        notificationService.NotifyChapterIssueCreatedAsync(mapper.Map<IssueChapterReportedDto>(issue));

        return Created();
    }

    [HttpPost("serie")]
    [RequirePermission(Permission.WriteIssues)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> CreateSerieIssue(IssueSerieReportedCreateDto issueSerieReportedCreate)
    {
        long? currentUserId = UserController.GetCurrentUserId(HttpContext);
        if (currentUserId == null) return Unauthorized();

        bool issueExists = await context.ReportedIssuesSerie
            .AnyAsync(i => i.UserId == currentUserId &&
                           i.SerieId == issueSerieReportedCreate.SerieId &&
                           i.ProblemId == issueSerieReportedCreate.ProblemId);

        if (issueExists) return Conflict("Issue already reported for this series and problem type.");

        IssueSerieReported issue = mapper.Map<IssueSerieReported>(issueSerieReportedCreate);
        issue.UserId = (long)currentUserId;
        issue.CreatedAt = DateTime.UtcNow;

        context.ReportedIssuesSerie.Add(issue);
        await context.SaveChangesAsync();
        notificationService.NotifySerieIssueCreatedAsync(mapper.Map<IssueSerieReportedDto>(issue));

        return Created();
    }

    [HttpPut("chapter/{id:long}/close")]
    [RequirePermission(Permission.DeleteIssues)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseChapterIssue(long id)
    {
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
        IssueSerieReported? issue = await context.ReportedIssuesSerie.FindAsync(id);

        if (issue == null) return NotFound(Localizer.IssueNotFound(id));

        context.ReportedIssuesSerie.Remove(issue);
        await context.SaveChangesAsync();
        notificationService.NotifySerieIssueDeletedAsync(issue.Id);

        return Ok();
    }
}