using System.Security.Claims;
using ManaxLibrary;
using ManaxLibrary.DTO.User;
using ManaxLibrary.Logging;
using ManaxServer.Attributes;
using ManaxServer.Models;
using ManaxServer.Models.Claim;
using ManaxServer.Models.User;
using ManaxServer.Services.Hash;
using ManaxServer.Services.Notification;
using ManaxServer.Services.Permission;
using ManaxServer.Services.Token;
using ManaxServer.Services.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

namespace ManaxServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController(
    ManaxContext context,
    IHashService hashService,
    ITokenService tokenService,
    INotificationService notificationService,
    IPermissionService permissionService,
    IPasswordValidationService passwordValidationService) : ControllerBase
{
    private readonly Lock _claimLock = new();

    [HttpGet("/api/users")]
    [RequirePermission(Permission.ReadUsers)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<long>>> GetUsers()
    {
        return await context.Users.Select(user => user.Id).ToListAsync();
    }

    [HttpGet("{id:long}")]
    [RequirePermission(Permission.ReadUsers)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUser(long id)
    {
        User? user = await context.Users.FindAsync(id);

        if (user == null) return NotFound(ErrorCode.UserDoesNotExist);

        return user.ToDto();
    }

    [HttpPut("update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutUser(UserUpdateDto userUpdate)
    {
        long? userId = GetCurrentUserId(HttpContext);
        if (userId == null)
            return Unauthorized(ErrorCode.TokenRequired);

        User? user = await context.Users.FindAsync(userId);
        if (user == null) return NotFound(ErrorCode.UserDoesNotExist);

        if (!passwordValidationService.IsPasswordValid(userUpdate.Password))
            return BadRequest(ErrorCode.InvalidPassword);

        user.PasswordHash = hashService.HashPassword(userUpdate.Password);
        user.Username = userUpdate.Username;
        notificationService.NotifyUserUpdatedAsync(user.ToDto());
        await context.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("{id:long}/password/reset")]
    [RequirePermission(Permission.ResetPasswords)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string>> ResetPassword(long id)
    {
        User? user = await context.Users.FindAsync(id);
        if (user == null) return NotFound(ErrorCode.UserDoesNotExist);

        string newPassword = passwordValidationService.GenerateValidPassword();

        user.PasswordHash = hashService.HashPassword(newPassword);
        await context.SaveChangesAsync();

        return newPassword;
    }

    [HttpPost("create")]
    [RequirePermission(Permission.WriteUsers)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> PostUser(UserCreateDto userCreate)
    {
        if (!passwordValidationService.IsPasswordValid(userCreate.Password))
            return BadRequest(ErrorCode.InvalidPassword);
        User user = Models.User.User.Create(userCreate);
        user.Creation = DateTime.UtcNow;
        user.PasswordHash = hashService.HashPassword(userCreate.Password);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        await permissionService.SetUserPermissionsAsync(user.Id,
            PermissionController.GetDefaultPermissionsForRole(user.Role));

        notificationService.NotifyUserCreatedAsync(user.ToDto());

        return Ok();
    }

    [HttpDelete("{id:long}")]
    [RequirePermission(Permission.DeleteUsers)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteUser(long id)
    {
        User? userToDelete = await context.Users.FindAsync(id);
        if (userToDelete == null) return NotFound(ErrorCode.UserDoesNotExist);

        long? selfId = GetCurrentUserId(HttpContext);
        if (selfId == null)
            return Unauthorized(ErrorCode.TokenRequired);
        if (selfId == id)
            return StatusCode(StatusCodes.Status403Forbidden, ErrorCode.CannotDeleteSelf);

        User? self = context.Users.FirstOrDefault(u => u.Id == selfId);
        if (self == null)
            return Unauthorized(ErrorCode.UserDoesNotExist);

        if (self.Role == UserRole.Admin && userToDelete.Role is UserRole.Admin or UserRole.Owner)
            return StatusCode(StatusCodes.Status403Forbidden, ErrorCode.InsufficientPermissions);

        StringValues auths = Request.Headers.Authorization;
        foreach (string? token in auths) tokenService.RevokeToken(token);

        context.Users.Remove(userToDelete);
        await context.SaveChangesAsync();
        notificationService.NotifyUserDeletedAsync(userToDelete.Id);

        return Ok();
    }

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
            Logger.LogWarning("Failed login attempt for user " + loginDto.Username + " from " + loginAttempt.Origin);
            context.LoginAttempts.Add(loginAttempt);
            await context.SaveChangesAsync();
            return Unauthorized(ErrorCode.InvalidPassword);
        }

        user.LastLogin = DateTime.UtcNow;
        loginAttempt.Success = true;
        Logger.LogInfo("User " + loginDto.Username + " logged in successfully from " + loginAttempt.Origin);
        context.LoginAttempts.Add(loginAttempt);
        await context.SaveChangesAsync();

        string token = tokenService.GenerateToken(user);
        UserLoginResultDto loginResult = new()
        {
            Token = token,
            User = user.ToDto()
        };
        return loginResult;
    }

    [HttpPost("/api/claim")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<UserLoginResultDto> Claim(ClaimRequest request)
    {
        if (!passwordValidationService.IsPasswordValid(request.Password))
            return BadRequest(ErrorCode.InvalidPassword);
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
                return Unauthorized(ErrorCode.ServerAlreadyClaimed);
            }

            User user = new()
            {
                Role = UserRole.Owner,
                Username = request.Username,
                PasswordHash = hashService.HashPassword(request.Password),
                Creation = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow
            };

            loginAttempt.Success = true;
            context.LoginAttempts.Add(loginAttempt);
            context.Users.Add(user);
            context.SaveChanges();

            permissionService.SetUserPermissions(user.Id, PermissionController.GetDefaultPermissionsForRole(user.Role));

            string token = tokenService.GenerateToken(user);
            UserLoginResultDto loginResult = new()
            {
                Token = token,
                User = user.ToDto()
            };
            return loginResult;
        }
    }

    [HttpPost("/api/logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        long? userId = GetCurrentUserId(HttpContext);
        User? user = await context.Users.FindAsync(userId);
        if (userId == null || user == null)
            return Unauthorized(ErrorCode.TokenRequired);

        StringValues auths = Request.Headers.Authorization;
        foreach (string? token in auths) tokenService.RevokeToken(token);

        LoginAttempt logoutAttempt = new()
        {
            Origin = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            Username = user.Username,
            Timestamp = DateTime.UtcNow,
            Type = "Logout",
            Success = true
        };

        Logger.LogInfo("User " + user.Username + " logged out from " + logoutAttempt.Origin);
        context.LoginAttempts.Add(logoutAttempt);
        await context.SaveChangesAsync();

        return Ok();
    }

    internal static long? GetCurrentUserId(HttpContext httpContext)
    {
        string? userIdClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !long.TryParse(userIdClaim, out long userId)) return null;
        return userId;
    }
}