using Microsoft.EntityFrameworkCore;

namespace ManaxApi.Models.Chapter;

public class ChapterContext(DbContextOptions<ChapterContext> options) : DbContext(options)
{
    public DbSet<Chapter> Chapters { get; set; } = null!;
}