using ManaxApi.Models.Issue;
using ManaxApi.Models.Rank;
using ManaxApi.Models.User;
using Microsoft.EntityFrameworkCore;

namespace ManaxApi.Models;

public class ManaxContext(DbContextOptions<ManaxContext> options) : DbContext(options)
{
    public DbSet<Chapter.Chapter> Chapters { get; set; } = null!;
    public DbSet<ChapterIssue> ChapterIssues { get; set; } = null!;
    public DbSet<ChapterIssueType> ChapterIssueTypes { get; set; } = null!;
    public DbSet<SerieIssue> SerieIssues { get; set; } = null!;
    public DbSet<SerieIssueType> SerieIssueTypes { get; set; } = null!;
    public DbSet<Library.Library> Libraries { get; set; } = null!;
    public DbSet<Read.Read> Reads { get; set; } = null!;
    public DbSet<Serie.Serie> Series { get; set; } = null!;
    public DbSet<User.User> Users { get; set; } = null!;
    public DbSet<LoginAttempt> LoginAttempts { get; set; } = null!;
    public DbSet<Rank.Rank> Ranks { get; set; } = null!;
    public DbSet<UserRank> UserRanks { get; set; } = null!;
}