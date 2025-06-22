using AutoMapper;
using ManaxApi.Auth;
using ManaxApi.Models;
using ManaxApi.Models.Issue;
using ManaxApi.Models.User;
using ManaxLibrary.DTOs;
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
        List<Issue> issues = await context.Issues.ToListAsync();
        return mapper.Map<List<IssueDTO>>(issues);
    }

    [HttpPost]
    [AuthorizeRole(UserRole.User)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(long))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<long>> CreateIssue(IssueCreateDTO issueCreate)
    {
        long? userId = UserController.GetCurrentUserId(HttpContext);
        if (userId == null) return Unauthorized();

        Issue? issue = mapper.Map<Issue>(issueCreate);
        issue.User = new User { Id = (long)userId };

        context.Issues.Add(issue);
        await context.SaveChangesAsync();
        
        return issue.Id;
    }

    [HttpPut("{id:long}/close")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseIssue(long id)
    {
        Issue? issue = await context.Issues.FindAsync(id);

        if (issue == null) return NotFound();

        context.Issues.Remove(issue);
        await context.SaveChangesAsync();

        return NoContent();
    }
}