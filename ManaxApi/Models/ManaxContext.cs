using Microsoft.EntityFrameworkCore;

namespace ManaxApi.Models;

public class ManaxContext(DbContextOptions<ManaxContext> options) : DbContext(options)
{
    public DbSet<Chapter.Chapter> Chapters { get; set; } = null!;
    public DbSet<Issue.Issue> Issues { get; set; } = null!;
    public DbSet<Library.Library> Libraries { get; set; } = null!;
    public DbSet<Read.Read> Reads { get; set; } = null!;
    public DbSet<Serie.Serie> Series { get; set; } = null!;
    public DbSet<User.User> Users { get; set; } = null!;
    public DbSet<Rank.Rank> Ranks { get; set; } = null!;
    public DbSet<Rank.UserRank> UserRanks { get; set; } = null!;
}