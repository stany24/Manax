using AutoMapper;
using ManaxApi.Auth;
using ManaxApi.Models;
using ManaxApi.Models.Issue.Internal;
using ManaxApi.Models.Issue.User;
using ManaxLibrary.DTOs.Issue.Internal;
using ManaxLibrary.DTOs.Issue.User;
using ManaxLibrary.DTOs.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxApi.Controllers;

[Route("api/issue")]
[ApiController]
public class IssueController(ManaxContext context, IMapper mapper) : ControllerBase
{
    [HttpGet("chapter/internal")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<InternalChapterIssueDTO>))]
    public async Task<ActionResult<IEnumerable<InternalChapterIssueDTO>>> GetAllInternalChapterIssues()
    {
        List<InternalChapterIssue> issues = await context.InternalChapterIssues.ToListAsync();
        return mapper.Map<List<InternalChapterIssueDTO>>(issues);
    }
    
    [HttpGet("serie/internal")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<InternalSerieIssueDTO>))]
    public async Task<ActionResult<IEnumerable<InternalSerieIssueDTO>>> GetAllInternalSerieIssues()
    {
        List<InternalSerieIssue> issues = await context.InternalSerieIssues.ToListAsync();
        return mapper.Map<List<InternalSerieIssueDTO>>(issues);
    }
    
    [HttpGet("chapter/user")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserChapterIssueDTO>))]
    public async Task<ActionResult<IEnumerable<UserChapterIssueDTO>>> GetAllUserChapterIssues()
    {
        List<UserChapterIssue> issues = await context.UserChapterIssues.ToListAsync();
        return mapper.Map<List<UserChapterIssueDTO>>(issues);
    }
    
    [HttpGet("serie/user")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserSerieIssueDTO>))]
    public async Task<ActionResult<IEnumerable<UserSerieIssueDTO>>> GetAllUserSerieIssues()
    {
        List<UserSerieIssue> issues = await context.UserSerieIssues.ToListAsync();
        return mapper.Map<List<UserSerieIssueDTO>>(issues);
    }

    [HttpPost("chapter")]
    [AuthorizeRole(UserRole.User)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> CreateChapterIssue(ChapterIssueCreateDTO chapterIssueCreate)
    {
        UserChapterIssue? issue = mapper.Map<UserChapterIssue>(chapterIssueCreate);

        context.UserChapterIssues.Add(issue);
        await context.SaveChangesAsync();

        return NoContent();
    }
    
    [HttpPost("serie")]
    [AuthorizeRole(UserRole.User)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> CreateSerieIssue(SerieIssueCreateDTO serieIssueCreate)
    {
        UserSerieIssue? issue = mapper.Map<UserSerieIssue>(serieIssueCreate);

        context.UserSerieIssues.Add(issue);
        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("chapter/{id:long}/close")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseChapterIssue(long id)
    {
        UserChapterIssue? issue = await context.UserChapterIssues.FindAsync(id);

        if (issue == null) return NotFound();

        context.UserChapterIssues.Remove(issue);
        await context.SaveChangesAsync();

        return NoContent();
    }
    
    [HttpPut("serie/{id:long}/close")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseSerieIssue(long id)
    {
        UserSerieIssue? issue = await context.UserSerieIssues.FindAsync(id);

        if (issue == null) return NotFound();

        context.UserSerieIssues.Remove(issue);
        await context.SaveChangesAsync();

        return NoContent();
    }
}