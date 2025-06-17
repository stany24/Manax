using Microsoft.EntityFrameworkCore;

namespace ManaxApi.Models.User;

public class UserContext(DbContextOptions<UserContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
}