using System.Security.Claims;
using ManaxLibrary.DTO.User;
using ManaxLibrary.Logging;
using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Models.Claim;
using ManaxServer.Models.User;
using ManaxServer.Services.Hash;
using ManaxServer.Services.Jwt;
using ManaxServer.Services.Mapper;
using ManaxServer.Services.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController(
    ManaxContext context,
    IMapper mapper,
    IHashService hashService,
    IJwtService jwtService,
    INotificationService notificationService) : ControllerBase
{
    private readonly object _claimLock = new();

    // GET: api/Users
    [HttpGet("/api/Users")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<long>>> GetUsers()
    {
        return await context.Users.Select(user => user.Id).ToListAsync();
    }

    // GET: api/User/5
    [HttpGet("{id:long}")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUser(long id)
    {
        User? user = await context.Users.FindAsync(id);

        if (user == null) return NotFound(Localizer.Format("UserNotFound", id));

        return mapper.Map<UserDto>(user);
    }

    // PUT: api/User/5
    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutUser(long id, UserUpdateDto userUpdate)
    {
        User? user = await context.Users.FindAsync(id);

        if (user == null) return NotFound(Localizer.Format("UserNotFound", id));

        mapper.Map(userUpdate, user);

        if (!string.IsNullOrEmpty(userUpdate.Password))
            user.PasswordHash = hashService.HashPassword(userUpdate.Password);

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException) when (!UserExists(id))
        {
            return NotFound();
        }

        return Ok();
    }

    // POST: api/User
    [HttpPost("create")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<long>> PostUser(UserCreateDto userCreate)
    {
        User user = mapper.Map<User>(userCreate);
        user.Creation = DateTime.UtcNow;
        user.PasswordHash = hashService.HashPassword(userCreate.Password);

        context.Users.Add(user);
        await context.SaveChangesAsync();
        notificationService.NotifyUserCreatedAsync(mapper.Map<UserDto>(user));

        return user.Id;
    }

    // DELETE: api/User/5
    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
        notificationService.NotifyUserDeletedAsync(userToDelete.Id);

        return Ok();
    }

    // POST: api/User/login
    [HttpPost("/api/login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserLoginResultDto>> Login(UserLoginDto loginDto)
    {
        if (!context.Users.Any())
            return Claim(new ClaimRequest { Username = loginDto.Username, Password = loginDto.Password });
        LoginAttempt loginAttempt = new()
        {
            Origin = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            Username = loginDto.Username,
            Timestamp = DateTime.UtcNow,
            Type = "Login",
            Success = false
        };
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Username == loginDto.Username);
        if (user == null || !hashService.VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            Logger.LogWarning("Failed login attempt for user " + loginDto.Username + " from " + loginAttempt.Origin,
                Environment.StackTrace);
            context.LoginAttempts.Add(loginAttempt);
            await context.SaveChangesAsync();
            return Unauthorized(Localizer.Format("UserInvalidLogin"));
        }

        user.LastLogin = DateTime.UtcNow;
        loginAttempt.Success = true;
        Logger.LogInfo("User " + loginDto.Username + " logged in successfully from " + loginAttempt.Origin);
        context.LoginAttempts.Add(loginAttempt);
        await context.SaveChangesAsync();

        string token = jwtService.GenerateToken(user);
        UserDto userDto = mapper.Map<UserDto>(user);
        UserLoginResultDto loginResult = new()
        {
            Token = token,
            User = userDto
        };
        return loginResult;
    }

    [HttpPost("/api/claim")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<UserLoginResultDto> Claim(ClaimRequest request)
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
                PasswordHash = hashService.HashPassword(request.Password)
            };

            loginAttempt.Success = true;
            context.LoginAttempts.Add(loginAttempt);
            context.Users.Add(user);
            context.SaveChanges();

            string token = jwtService.GenerateToken(user);
            UserDto userDto = mapper.Map<UserDto>(user);
            UserLoginResultDto loginResult = new()
            {
                Token = token,
                User = userDto
            };
            return loginResult;
        }
    }

    internal static long? GetCurrentUserId(HttpContext httpContext)
    {
        string? userIdClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !long.TryParse(userIdClaim, out long userId)) return null;
        return userId;
    }

    private bool UserExists(long id)
    {
        return context.Users.Any(e => e.Id == id);
    }
}