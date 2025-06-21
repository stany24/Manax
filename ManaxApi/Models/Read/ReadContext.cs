using Microsoft.EntityFrameworkCore;

namespace ManaxApi.Models.Read;

public class ReadContext(DbContextOptions<ReadContext> options) : DbContext(options)
{
    public DbSet<Read> Reads { get; set; } = null!;
}