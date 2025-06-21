using ManaxApi.Auth;
using ManaxApi.Models.Chapter;
using ManaxApi.Models.Issue;
using ManaxApi.Models.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IssueController(ManaxContext ManaxContext) : ControllerBase
{
    [HttpGet]
    [AuthorizeRole(UserRole.Admin)]
    public async Task<ActionResult<IEnumerable<Issue>>> GetAllIssues()
    {
        return await ManaxContext.Issues
            .ToListAsync();
    }

    [HttpPost]
    [AuthorizeRole(UserRole.User)]
    public async Task<ActionResult<Issue>> CreateIssue(Issue issue)
    {
        long? userId = UserController.GetCurrentUserId(HttpContext);
        if (userId == null) return Unauthorized();

        issue.User = new User { Id = (long)userId };

        ManaxContext.Issues.Add(issue);
        await ManaxContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAllIssues), new { id = issue.Id }, issue);
    }

    [HttpPut("{id:long}/close")]
    [AuthorizeRole(UserRole.Admin)]
    public async Task<IActionResult> CloseIssue(long id)
    {
        Issue? issue = await ManaxContext.Issues.FindAsync(id);

        if (issue == null) return NotFound();

        ManaxContext.Issues.Remove(issue);
        await ManaxContext.SaveChangesAsync();

        return NoContent();
    }
}