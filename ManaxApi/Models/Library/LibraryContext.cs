using Microsoft.EntityFrameworkCore;

namespace ManaxApi.Models.Library;

public class LibraryContext(DbContextOptions<LibraryContext> options) : DbContext(options)
{
    public DbSet<Library> Libraries { get; set; } = null!;
}