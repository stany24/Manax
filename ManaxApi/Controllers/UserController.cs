using ManaxApi.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ManaxApi.Models.User;

namespace ManaxApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(UserContext context) : ControllerBase
    {
        // GET: api/Users
        [HttpGet("/api/Users")]
        [AuthorizeRole(UserRole.User)]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await context.Users.ToListAsync();
        }

        // GET: api/User/5
        [HttpGet("{id:long}")]
        [AuthorizeRole(UserRole.User)]
        public async Task<ActionResult<User>> GetUser(long id)
        {
            User? user = await context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/User/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id:long}")]
        [AuthorizeRole(UserRole.User)]
        public async Task<IActionResult> PutUser(long id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            context.Entry(user).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        // POST: api/User
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            // Vérifie si la base est vide
            if (!context.Users.Any())
            {
                user.Role = UserRole.Owner;
                context.Users.Add(user);
                await context.SaveChangesAsync();
                return CreatedAtAction("GetUser", new { id = user.Id }, user);
            }
            // Si la base n'est pas vide, nécessite l'authentification et le rôle User
            if (!(HttpContext.User.Identity?.IsAuthenticated ?? false))
                return Unauthorized();
            string? roleClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
            if (roleClaim == null || !Enum.TryParse(roleClaim, out UserRole userRole) || userRole < UserRole.User)
                return Forbid();
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        // DELETE: api/User/5
        [HttpDelete("{id:long}")]
        [AuthorizeRole(UserRole.User)]
        public async Task<IActionResult> DeleteUser(long id)
        {
            User? user = await context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            context.Users.Remove(user);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(long id)
        {
            return context.Users.Any(e => e.Id == id);
        }
    }
}

