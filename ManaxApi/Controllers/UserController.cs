using System.Security.Claims;
using AutoMapper;
using ManaxApi.Auth;
using ManaxApi.Models;
using ManaxApi.Models.User;
using ManaxApi.Services;
using ManaxLibrary.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController(ManaxContext context, IMapper mapper, IConfiguration config) : ControllerBase
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDTO>> GetUser(long id)
    {
        User? user = await context.Users.FindAsync(id);

        if (user == null) return NotFound();

        return mapper.Map<UserDTO>(user);
    }

    // PUT: api/User/5
    [HttpPut("{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PutUser(long id, UserUpdateDTO userDTO)
    {
        User? user = await context.Users.FindAsync(id);
        
        if (user == null) return NotFound();
        
        mapper.Map(userDTO, user);
        
        // Si un nouveau mot de passe est fourni, le hasher
        if (!string.IsNullOrEmpty(userDTO.Password))
        {
            user.PasswordHash = HashService.ComputeSha3_512(userDTO.Password);
        }

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
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserDTO))]
    public async Task<ActionResult<UserDTO>> PostUser(UserCreateDTO userCreateDTO)
    {
        User? user = mapper.Map<User>(userCreateDTO);
        
        // Hasher le mot de passe
        user.PasswordHash = HashService.ComputeSha3_512(userCreateDTO.Password);
        
        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        UserDTO? userDTO = mapper.Map<UserDTO>(user);

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userDTO);
    }

    // DELETE: api/User/5
    [HttpDelete("{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    public async Task<IActionResult> Login(UserLoginDTO loginDTO)
    {
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Username == loginDTO.Username);
        if (user == null)
            return Unauthorized();
        string hash = HashService.ComputeSha3_512(loginDTO.Password);
        if (user.PasswordHash != hash)
            return Unauthorized();
        string token = JwtService.GenerateToken(user, config);
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
        if (!(HttpContext.User.Identity?.IsAuthenticated ?? false))
            return Unauthorized();

        long? userId = GetCurrentUserId(HttpContext);
        if (userId == null) return Unauthorized();

        User? user = await context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        return mapper.Map<UserDTO>(user);
    }

    [HttpPost("/api/claim")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> Claim(LoginRequest request)
    {
        if (context.Users.Any()) return Forbid();

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
        if (userIdClaim == null || !long.TryParse(userIdClaim, out long userId)) return null;
        return userId;
    }
}