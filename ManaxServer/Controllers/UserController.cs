using System.Security.Claims;
using AutoMapper;
using ManaxLibrary.DTOs.User;
using ManaxLibrary.Logging;
using ManaxServer.Auth;
using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Models.User;
using ManaxServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController(ManaxContext context, IMapper mapper) : ControllerBase
{
    private readonly object _claimLock = new();

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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDTO>> GetUser(long id)
    {
        User? user = await context.Users.FindAsync(id);

        if (user == null) return NotFound(Localizer.Format("UserNotFound", id));

        return mapper.Map<UserDTO>(user);
    }

    // PUT: api/User/5
    [HttpPut("{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PutUser(long id, UserUpdateDTO userUpdate)
    {
        User? user = await context.Users.FindAsync(id);

        if (user == null) return NotFound(Localizer.Format("UserNotFound", id));

        mapper.Map(userUpdate, user);

        if (!string.IsNullOrEmpty(userUpdate.Password))
            user.PasswordHash = HashService.HashPassword(userUpdate.Password);

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
    [HttpPost("create")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(long))]
    public async Task<ActionResult<long>> PostUser(UserCreateDTO userCreate)
    {
        User? user = mapper.Map<User>(userCreate);
        user.PasswordHash = HashService.HashPassword(userCreate.Password);

        context.Users.Add(user);
        await context.SaveChangesAsync();
        NotificationService.NotifyUserCreatedAsync(mapper.Map<UserDTO>(user));

        return user.Id;
    }

    // DELETE: api/User/5
    [HttpDelete("{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteUser(long id)
    {
        User? userToDelete = await context.Users.FindAsync(id);
        if (userToDelete == null) return NotFound(Localizer.Format("UserNotFound", id));

        long? selfId = GetCurrentUserId(HttpContext);
        if (selfId == null)
            return Unauthorized(Localizer.Format("UserMustBeLoggedInDelete"));
        if (selfId == id)
            return Forbid(Localizer.Format("UserCannotDeleteSelf"));

        User? self = context.Users.FirstOrDefault(u => u.Id == selfId);
        if (self == null)
            return Unauthorized(Localizer.Format("UserMustBeLoggedInDelete"));

        if (self.Role == UserRole.Admin && userToDelete.Role is UserRole.Admin or UserRole.Owner)
            return Forbid(Localizer.Format("UserCannotDeleteAdminOrOwner"));

        context.Users.Remove(userToDelete);
        await context.SaveChangesAsync();
        NotificationService.NotifyUserDeletedAsync(userToDelete.Id);

        return NoContent();
    }

    // POST: api/User/login
    [HttpPost("/api/login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(UserLoginDTO loginDto)
    {
        LoginAttempt loginAttempt = new()
        {
            Origin = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            Username = loginDto.Username,
            Timestamp = DateTime.UtcNow,
            Type = "Login",
            Success = false
        };
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Username == loginDto.Username);
        if (user == null || !HashService.VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            Logger.LogWarning("Failed login attempt for user "+loginDto.Username+" from "+loginAttempt.Origin,Environment.StackTrace);
            context.LoginAttempts.Add(loginAttempt);
            await context.SaveChangesAsync();
            return Unauthorized(Localizer.Format("UserInvalidLogin"));
        }
        
        loginAttempt.Success = true;
        Logger.LogInfo("User "+loginDto.Username+" logged in successfully from "+loginAttempt.Origin);
        context.LoginAttempts.Add(loginAttempt);
        await context.SaveChangesAsync();

        string token = JwtService.GenerateToken(user);
        return Ok(new { token });
    }

    // GET: api/User/current
    [HttpGet("current")]
    [AuthorizeRole(UserRole.User)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDTO>> GetCurrentUser()
    {
        long? userId = GetCurrentUserId(HttpContext);
        if (userId == null) return Unauthorized(Localizer.Format("UserMustBeLoggedInInfo"));

        User? user = await context.Users.FindAsync(userId);
        if (user == null) return NotFound(Localizer.Format("UserNotFound", userId));

        return mapper.Map<UserDTO>(user);
    }

    [HttpPost("/api/claim")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult Claim(LoginRequest request)
    {
        LoginAttempt loginAttempt = new()
        {
            Origin = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            Username = request.Username,
            Timestamp = DateTime.UtcNow,
            Type = "Claim",
            Success = false
        };

        lock (_claimLock)
        {
            if (context.Users.Any())
            {
                context.LoginAttempts.Add(loginAttempt);
                context.SaveChanges();
                return Unauthorized(Localizer.Format("UserClaimNotAllowed"));
            }

            User user = new()
            {
                Role = UserRole.Owner,
                Username = request.Username,
                PasswordHash = HashService.HashPassword(request.Password)
            };

            loginAttempt.Success = true;
            context.LoginAttempts.Add(loginAttempt);
            context.Users.Add(user);
            context.SaveChanges();

            string token = JwtService.GenerateToken(user);
            return Ok(new { token });
        }
    }

    internal static long? GetCurrentUserId(HttpContext httpContext)
    {
        string? userIdClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !long.TryParse(userIdClaim, out long userId)) return null;
        return userId;
    }
}