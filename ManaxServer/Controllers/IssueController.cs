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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AutomaticIssueChapterDto>))]
    public async Task<ActionResult<IEnumerable<AutomaticIssueChapterDto>>> GetAllAutomaticChapterIssues()
    {
        List<AutomaticIssueChapter> issues = await context.AutomaticIssuesChapter.ToListAsync();
        return mapper.Map<List<AutomaticIssueChapterDto>>(issues);
    }
    
    [HttpGet("serie/automatic")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AutomaticIssueSerieDto>))]
    public async Task<ActionResult<IEnumerable<AutomaticIssueSerieDto>>> GetAllAutomaticSerieIssues()
    {
        List<AutomaticIssueSerie> issues = await context.AutomaticIssuesSerie.ToListAsync();
        return mapper.Map<List<AutomaticIssueSerieDto>>(issues);
    }
    
    [HttpGet("chapter/reported")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ReportedIssueChapterDto>))]
    public async Task<ActionResult<IEnumerable<ReportedIssueChapterDto>>> GetAllReportedChapterIssues()
    {
        List<ReportedIssueChapter> issues = await context.ReportedIssuesChapter.ToListAsync();
        return mapper.Map<List<ReportedIssueChapterDto>>(issues);
    }
    
    [HttpGet("serie/reported")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ReportedIssueSerieDto>))]
    public async Task<ActionResult<IEnumerable<ReportedIssueSerieDto>>> GetAllReportedSerieIssues()
    {
        List<ReportedIssueSerie> issues = await context.ReportedIssuesSerie.ToListAsync();
        return mapper.Map<List<ReportedIssueSerieDto>>(issues);
    }

    [HttpPost("chapter")]
    [Authorize(Roles = "User,Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> CreateChapterIssue(ReportedIssueChapterCreateDto reportedIssueChapterCreate)
    {
        ReportedIssueChapter? issue = mapper.Map<ReportedIssueChapter>(reportedIssueChapterCreate);

        context.ReportedIssuesChapter.Add(issue);
        await context.SaveChangesAsync();

        return NoContent();
    }
    
    [HttpPost("serie")]
    [Authorize(Roles = "User,Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> CreateSerieIssue(ReportedIssueSerieCreateDto reportedIssueSerieCreate)
    {
        ReportedIssueSerie? issue = mapper.Map<ReportedIssueSerie>(reportedIssueSerieCreate);

        context.ReportedIssuesSerie.Add(issue);
        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("chapter/{id:long}/close")]
    [Authorize(Roles = "Admin,Owner")]
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
    [Authorize(Roles = "Admin,Owner")]
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