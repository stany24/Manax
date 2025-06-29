using AutoMapper;
using ManaxApi.Auth;
using ManaxApi.Models;
using ManaxApi.Models.Issue;
using ManaxApi.Models.User;
using ManaxLibrary.DTOs;
using ManaxLibrary.DTOs.Issue;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IssueController(ManaxContext context, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<IssueDTO>))]
    public async Task<ActionResult<IEnumerable<IssueDTO>>> GetAllIssues()
    {
        List<ChapterIssue> issues = await context.ChapterIssues.ToListAsync();
        return mapper.Map<List<IssueDTO>>(issues);
    }

    [HttpPost]
    [AuthorizeRole(UserRole.User)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> CreateIssue(IssueCreateDTO issueCreate)
    {
        ChapterIssue? issue = mapper.Map<ChapterIssue>(issueCreate);

        context.ChapterIssues.Add(issue);
        await context.SaveChangesAsync();
        
        return NoContent();
    }

    [HttpPut("{id:long}/close")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseIssue(long id)
    {
        ChapterIssue? issue = await context.ChapterIssues.FindAsync(id);

        if (issue == null) return NotFound();

        context.ChapterIssues.Remove(issue);
        await context.SaveChangesAsync();

        return NoContent();
    }
}