using System.Security.Claims;
using ManaxApi.Auth;
using ManaxApi.Models.User;
using ManaxApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController(UserContext context, IConfiguration config) : ControllerBase
{
    // GET: api/Users
    [HttpGet("/api/Users")]
    [AuthorizeRole(UserRole.Admin)]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return await context.Users.ToListAsync();
    }

    // GET: api/User/5
    [HttpGet("{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    public async Task<ActionResult<User>> GetUser(long id)
    {
        User? user = await context.Users.FindAsync(id);

        if (user == null) return NotFound();

        return user;
    }

    // PUT: api/User/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    public async Task<IActionResult> PutUser(long id, User user)
    {
        if (id != user.Id) return BadRequest();

        context.Entry(user).State = EntityState.Modified;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!context.Users.Any(e => e.Id == id)) return NotFound();

            throw;
        }

        return NoContent();
    }

    // POST: api/User
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost("create")]
    [AuthorizeRole(UserRole.Admin)]
    public async Task<ActionResult<User>> PostUser(User user)
    {
        if (!context.Users.Any())
        {
            user.Role = UserRole.Owner;
            user.PasswordHash = HashService.ComputeSha3_512(user.PasswordHash);
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        if (!(HttpContext.User.Identity?.IsAuthenticated ?? false))
            return Unauthorized();
        string? roleClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        if (roleClaim == null || !Enum.TryParse(roleClaim, out UserRole userRole) || userRole < UserRole.Admin)
            return Forbid();
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return CreatedAtAction("GetUser", new { id = user.Id }, user);
    }

    // DELETE: api/User/5
    [HttpDelete("{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    public async Task<IActionResult> DeleteUser(long id)
    {
        User? user = await context.Users.FindAsync(id);
        if (user == null) return NotFound();

        context.Users.Remove(user);
        await context.SaveChangesAsync();

        return NoContent();
    }

    // POST: api/User/login
    [HttpPost("/login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user == null)
            return Unauthorized();
        string hash = HashService.ComputeSha3_512(request.Password);
        if (user.PasswordHash != hash)
            return Unauthorized();
        string token = JwtService.GenerateToken(user, config);
        return Ok(new { token });
    }
}