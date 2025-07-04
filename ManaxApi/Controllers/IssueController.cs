using AutoMapper;
using ManaxApi.Auth;
using ManaxApi.Models;
using ManaxApi.Models.Issue.User;
using ManaxLibrary.DTOs.Issue;
using ManaxLibrary.DTOs.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxApi.Controllers;

[Route("api/issue")]
[ApiController]
public class IssueController(ManaxContext context, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<IssueDTO>))]
    public async Task<ActionResult<IEnumerable<IssueDTO>>> GetAllIssues()
    {
        List<UserChapterIssue> issues = await context.UserChapterIssues.ToListAsync();
        return mapper.Map<List<IssueDTO>>(issues);
    }

    [HttpPost]
    [AuthorizeRole(UserRole.User)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> CreateIssue(IssueCreateDTO issueCreate)
    {
        UserChapterIssue? issue = mapper.Map<UserChapterIssue>(issueCreate);

        context.UserChapterIssues.Add(issue);
        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{id:long}/close")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseIssue(long id)
    {
        UserChapterIssue? issue = await context.UserChapterIssues.FindAsync(id);

        if (issue == null) return NotFound();

        context.UserChapterIssues.Remove(issue);
        await context.SaveChangesAsync();

        return NoContent();
    }
}