using ManaxLibrary.DTO.Issue.Automatic;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Models.Issue.Reported;
using ManaxServer.Services.Mapper;
using ManaxServer.Services.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/issue")]
[ApiController]
public class IssueController(ManaxContext context, IMapper mapper, INotificationService notificationService)
    : ControllerBase
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
        return await context.ReportedIssueSerieTypes.ToListAsync();
    }

    [HttpPost("chapter")]
    [Authorize(Roles = "User,Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> CreateChapterIssue(ReportedIssueChapterCreateDto reportedIssueChapterCreate)
    {
        long? currentUserId = UserController.GetCurrentUserId(HttpContext);
        if (currentUserId == null) return Unauthorized();

        bool issueExists = await context.ReportedIssuesChapter
            .AnyAsync(i => i.UserId == currentUserId && 
                          i.ChapterId == reportedIssueChapterCreate.ChapterId && 
                          i.ProblemId == reportedIssueChapterCreate.ProblemId);

        if (issueExists)
        {
            return Conflict("Issue already reported for this chapter and problem type.");
        }

        ReportedIssueChapter issue = mapper.Map<ReportedIssueChapter>(reportedIssueChapterCreate);
        issue.UserId = (long)currentUserId;
        issue.CreatedAt = DateTime.UtcNow;

        context.ReportedIssuesChapter.Add(issue);
        await context.SaveChangesAsync();
        notificationService.NotifyChapterIssueCreatedAsync(mapper.Map<ReportedIssueChapterDto>(issue));

        return Created();
    }

    [HttpPost("serie")]
    [Authorize(Roles = "User,Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> CreateSerieIssue(ReportedIssueSerieCreateDto reportedIssueSerieCreate)
    {
        long? currentUserId = UserController.GetCurrentUserId(HttpContext);
        if (currentUserId == null) return Unauthorized();

        bool issueExists = await context.ReportedIssuesSerie
            .AnyAsync(i => i.UserId == currentUserId && 
                          i.SerieId == reportedIssueSerieCreate.SerieId && 
                          i.ProblemId == reportedIssueSerieCreate.ProblemId);

        if (issueExists)
        {
            return Conflict("Issue already reported for this series and problem type.");
        }

        ReportedIssueSerie issue = mapper.Map<ReportedIssueSerie>(reportedIssueSerieCreate);
        issue.UserId = (long)currentUserId;
        issue.CreatedAt = DateTime.UtcNow;

        context.ReportedIssuesSerie.Add(issue);
        await context.SaveChangesAsync();
        notificationService.NotifySerieIssueCreatedAsync(mapper.Map<ReportedIssueSerieDto>(issue));

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
        notificationService.NotifyChapterIssueDeletedAsync(issue.Id);

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
        notificationService.NotifySerieIssueDeletedAsync(issue.Id);

        return Ok();
    }
}