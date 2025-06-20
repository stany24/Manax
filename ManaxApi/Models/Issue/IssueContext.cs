using Microsoft.EntityFrameworkCore;

namespace ManaxApi.Models.Issue;

public class IssueContext(DbContextOptions<IssueContext> options) : DbContext(options)
{
    public DbSet<Issue> Issues { get; set; } = null!;
}