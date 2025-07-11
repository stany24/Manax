using ManaxApi.Models.Issue.Automatic;
using ManaxApi.Models.Issue.Reported;
using ManaxApi.Models.Rank;
using ManaxApi.Models.User;
using Microsoft.EntityFrameworkCore;

namespace ManaxApi.Models;

public class ManaxContext(DbContextOptions<ManaxContext> options) : DbContext(options)
{
    public DbSet<Library.Library> Libraries { get; set; } = null!;
    public DbSet<Serie.Serie> Series { get; set; } = null!;
    public DbSet<Chapter.Chapter> Chapters { get; set; } = null!;
    
    public DbSet<AutomaticIssueChapter> AutomaticIssuesChapter { get; set; } = null!;
    public DbSet<AutomaticIssueSerie> AutomaticIssuesSerie { get; set; } = null!;
    public DbSet<ReportedIssueChapter> ReportedIssuesChapter { get; set; } = null!;
    public DbSet<ReportedIssueChapterType> ReportedIssueChapterTypes { get; set; } = null!;
    public DbSet<ReportedIssueSerie> ReportedIssuesSerie { get; set; } = null!;
    public DbSet<ReportedIssueSerieType> ReportedIssueSerieTypes { get; set; } = null!;
    
    public DbSet<User.User> Users { get; set; } = null!;
    public DbSet<Read.Read> Reads { get; set; } = null!;
    public DbSet<LoginAttempt> LoginAttempts { get; set; } = null!;
    public DbSet<Rank.Rank> Ranks { get; set; } = null!;
    public DbSet<UserRank> UserRanks { get; set; } = null!;
}