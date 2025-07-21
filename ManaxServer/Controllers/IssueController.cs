using AutoMapper;
using ManaxLibrary.DTOs.Issue.Automatic;
using ManaxLibrary.DTOs.Issue.Reported;
using ManaxLibrary.DTOs.User;
using ManaxServer.Auth;
using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Models.Issue.Automatic;
using ManaxServer.Models.Issue.Reported;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/issue")]
[ApiController]
public class IssueController(ManaxContext context, IMapper mapper) : ControllerBase
{
    [HttpGet("chapter/automatic")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AutomaticIssueChapterDTO>))]
    public async Task<ActionResult<IEnumerable<AutomaticIssueChapterDTO>>> GetAllAutomaticChapterIssues()
    {
        List<AutomaticIssueChapter> issues = await context.AutomaticIssuesChapter.ToListAsync();
        return mapper.Map<List<AutomaticIssueChapterDTO>>(issues);
    }
    
    [HttpGet("serie/automatic")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AutomaticIssueSerieDTO>))]
    public async Task<ActionResult<IEnumerable<AutomaticIssueSerieDTO>>> GetAllAutomaticSerieIssues()
    {
        List<AutomaticIssueSerie> issues = await context.AutomaticIssuesSerie.ToListAsync();
        return mapper.Map<List<AutomaticIssueSerieDTO>>(issues);
    }
    
    [HttpGet("chapter/reported")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ReportedIssueChapterDTO>))]
    public async Task<ActionResult<IEnumerable<ReportedIssueChapterDTO>>> GetAllReportedChapterIssues()
    {
        List<ReportedIssueChapter> issues = await context.ReportedIssuesChapter.ToListAsync();
        return mapper.Map<List<ReportedIssueChapterDTO>>(issues);
    }
    
    [HttpGet("serie/reported")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ReportedIssueSerieDTO>))]
    public async Task<ActionResult<IEnumerable<ReportedIssueSerieDTO>>> GetAllReportedSerieIssues()
    {
        List<ReportedIssueSerie> issues = await context.ReportedIssuesSerie.ToListAsync();
        return mapper.Map<List<ReportedIssueSerieDTO>>(issues);
    }

    [HttpPost("chapter")]
    [AuthorizeRole(UserRole.User)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> CreateChapterIssue(ReportedIssueChapterCreateDTO reportedIssueChapterCreate)
    {
        ReportedIssueChapter? issue = mapper.Map<ReportedIssueChapter>(reportedIssueChapterCreate);

        context.ReportedIssuesChapter.Add(issue);
        await context.SaveChangesAsync();

        return NoContent();
    }
    
    [HttpPost("serie")]
    [AuthorizeRole(UserRole.User)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> CreateSerieIssue(ReportedIssueSerieCreateDTO reportedIssueSerieCreate)
    {
        ReportedIssueSerie? issue = mapper.Map<ReportedIssueSerie>(reportedIssueSerieCreate);

        context.ReportedIssuesSerie.Add(issue);
        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("chapter/{id:long}/close")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseChapterIssue(long id)
    {
        ReportedIssueChapter? issue = await context.ReportedIssuesChapter.FindAsync(id);

        if (issue == null) return NotFound(Localizer.Format("IssueNotFound", id));

        context.ReportedIssuesChapter.Remove(issue);
        await context.SaveChangesAsync();

        return NoContent();
    }
    
    [HttpPut("serie/{id:long}/close")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseSerieIssue(long id)
    {
        ReportedIssueSerie? issue = await context.ReportedIssuesSerie.FindAsync(id);

        if (issue == null) return NotFound(Localizer.Format("IssueNotFound", id));

        context.ReportedIssuesSerie.Remove(issue);
        await context.SaveChangesAsync();

        return NoContent();
    }
}