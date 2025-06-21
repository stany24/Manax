using ManaxApi.Auth;
using ManaxApi.Models.Issue;
using ManaxApi.Models.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IssueController(IssueContext issueContext) : ControllerBase
{
    [HttpGet]
    [AuthorizeRole(UserRole.Admin)]
    public async Task<ActionResult<IEnumerable<Issue>>> GetAllIssues()
    {
        return await issueContext.Issues
            .ToListAsync();
    }

    [HttpPost]
    [AuthorizeRole(UserRole.User)]
    public async Task<ActionResult<Issue>> CreateIssue(Issue issue)
    {
        long? userId = UserController.GetCurrentUserId(HttpContext);
        if (userId == null) return Unauthorized();

        issue.User = new User { Id = (long)userId };

        issueContext.Issues.Add(issue);
        await issueContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAllIssues), new { id = issue.Id }, issue);
    }

    [HttpPut("{id:long}/close")]
    [AuthorizeRole(UserRole.Admin)]
    public async Task<IActionResult> CloseIssue(long id)
    {
        Issue? issue = await issueContext.Issues.FindAsync(id);

        if (issue == null) return NotFound();

        issueContext.Issues.Remove(issue);
        await issueContext.SaveChangesAsync();

        return NoContent();
    }
}