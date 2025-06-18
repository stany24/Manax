using Microsoft.EntityFrameworkCore;

namespace ManaxApi.Models.Serie;

public class SerieContext(DbContextOptions<SerieContext> options) : DbContext(options)
{
    public DbSet<Serie> Series { get; set; } = null!;
}