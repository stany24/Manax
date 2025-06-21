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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<long>))]
    public async Task<ActionResult<IEnumerable<long>>> GetUsers()
    {
        return await context.Users.Select(user => user.Id).ToListAsync();
    }

    // GET: api/User/5
    [HttpGet("{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserInfo))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserInfo>> GetUser(long id)
    {
        User? user = await context.Users.FindAsync(id);

        if (user == null) return NotFound();

        return user.GetInfo();
    }

    // PUT: api/User/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(long))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<long>> PostUser(User user)
    {
        user.PasswordHash = HashService.ComputeSha3_512(user.PasswordHash);
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user.Id;
    }

    // DELETE: api/User/5
    [HttpDelete("{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteUser(long id)
    {
        User? user = await context.Users.FindAsync(id);
        if (user == null) return NotFound();

        context.Users.Remove(user);
        await context.SaveChangesAsync();

        return NoContent();
    }

    // POST: api/User/login
    [HttpPost("/api/login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    // GET: api/User/current
    [HttpGet("current")]
    [AuthorizeRole(UserRole.User)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserInfo))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserInfo>> GetCurrentUser()
    {
        if (!(HttpContext.User.Identity?.IsAuthenticated ?? false))
            return Unauthorized();
            
        long? userId = GetCurrentUserId(HttpContext);
        if (userId == null) {return Unauthorized();}
            
        User? user = await context.Users.FindAsync(userId);
        if (user == null) return NotFound();
            
        return user.GetInfo();
    }

    [HttpPost("/api/claim")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> Claim(LoginRequest request)
    {
        if (context.Users.Any()) { return Forbid(); }

        User user = new()
        {
            Role = UserRole.Owner,
            Username = request.Username,
            PasswordHash = HashService.ComputeSha3_512(request.Password)
        };
        
        context.Users.Add(user);
        await context.SaveChangesAsync();
            
        string token = JwtService.GenerateToken(user, config);
        return Ok(new { token });
    }

    internal static long? GetCurrentUserId(HttpContext httpContext)
    {
        string? userIdClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !long.TryParse(userIdClaim, out long userId))
        {
            return null;
        }
        return userId;
    }
}