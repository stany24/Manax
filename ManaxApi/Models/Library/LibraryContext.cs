using Microsoft.EntityFrameworkCore;

namespace ManaxApi.Models;

public class LibraryContext(DbContextOptions<LibraryContext> options) : DbContext(options)
{
    public DbSet<Library> Libraries { get; set; } = null!;
}