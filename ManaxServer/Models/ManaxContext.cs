using ManaxServer.Models.Issue.Automatic;
using ManaxServer.Models.Issue.Reported;
using ManaxServer.Models.Rank;
using ManaxServer.Models.User;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Models;

public class ManaxContext(DbContextOptions<ManaxContext> options) : DbContext(options)
{
    public virtual DbSet<SavePoint.SavePoint> SavePoints { get; set; } = null!;
    public virtual DbSet<Library.Library> Libraries { get; set; } = null!;
    public virtual DbSet<Serie.Serie> Series { get; set; } = null!;
    public virtual DbSet<Chapter.Chapter> Chapters { get; set; } = null!;

    public virtual DbSet<IssueChapterAutomatic> AutomaticIssuesChapter { get; set; } = null!;
    public virtual DbSet<AutomaticIssueSerie> AutomaticIssuesSerie { get; set; } = null!;
    public virtual DbSet<IssueChapterReported> ReportedIssuesChapter { get; set; } = null!;
    public virtual DbSet<IssueChapterReportedType> ReportedIssueChapterTypes { get; set; } = null!;
    public virtual DbSet<IssueSerieReported> ReportedIssuesSerie { get; set; } = null!;
    public virtual DbSet<IssueSerieReportedType> ReportedIssueSerieTypes { get; set; } = null!;

    public virtual DbSet<User.User> Users { get; set; } = null!;
    public virtual DbSet<UserPermission> UserPermissions { get; set; } = null!;
    public virtual DbSet<Read.Read> Reads { get; set; } = null!;
    public virtual DbSet<LoginAttempt> LoginAttempts { get; set; } = null!;
    public virtual DbSet<UserRank> UserRanks { get; set; } = null!;

    public virtual DbSet<Rank.Rank> Ranks { get; set; } = null!;
    public virtual DbSet<Tag.Tag> Tags { get; set; } = null!;
}