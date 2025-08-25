using ManaxLibrary.DTO.Serie;
using ManaxLibrary.DTO.User;
using ManaxServer.Models;
using ManaxServer.Models.Chapter;
using ManaxServer.Models.Library;
using ManaxServer.Models.SavePoint;
using ManaxServer.Models.Serie;
using ManaxServer.Models.Rank;
using ManaxServer.Models.User;
using Microsoft.EntityFrameworkCore;

namespace ManaxTests.Mocks;

public static class SqliteTestDbContextFactory
{
    public static ManaxContext CreateTestContext(string? dbName = null)
    {
        dbName ??= Guid.NewGuid().ToString();
        string dbPath = Path.Combine(Path.GetTempPath(), $"test_{dbName}.db");

        DbContextOptions<ManaxContext> options = new DbContextOptionsBuilder<ManaxContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        List<Library> libraries =
        [
            new()
            {
                Id = 1,
                Name = "Library 1",
                Creation = DateTime.UtcNow
            },

            new()
            {
                Id = 2,
                Name = "Library 2",
                Creation = DateTime.UtcNow
            }
        ];

        List<Serie> series =
        [
            new()
            {
                Id = 1,
                LibraryId = 1,
                FolderName = "serie1",
                Title = "Serie 1",
                Description = "Description for Serie 1",
                SavePointId = 1,
                Status = Status.Ongoing,
                Creation = DateTime.UtcNow,
                LastModification = DateTime.UtcNow
            },

            new()
            {
                Id = 2,
                LibraryId = 1,
                FolderName = "serie2",
                Title = "Serie 2",
                Description = "Description for Serie 2",
                SavePointId = 1,
                Status = Status.Completed,
                Creation = DateTime.UtcNow,
                LastModification = DateTime.UtcNow
            },
            
            new()
            {
                Id = 3,
                LibraryId = 1,
                FolderName = "serie3",
                Title = "Serie 3",
                Description = "Description for Serie 3",
                SavePointId = 1,
                Status = Status.Completed,
                Creation = DateTime.UtcNow,
                LastModification = DateTime.UtcNow
            }
        ];

        List<Chapter> chapters =
        [
            new()
            {
                Id = 1,
                SerieId = 1,
                FileName = "chapter1.cbz",
                Number = 1,
                Pages = 24,
                Path = "/test_path/serie1/chapter1.cbz",
                Creation = DateTime.UtcNow,
                LastModification = DateTime.UtcNow
            },

            new()
            {
                Id = 2,
                SerieId = 1,
                FileName = "chapter2.cbz",
                Number = 2,
                Pages = 30,
                Path = "/test_path/serie1/chapter2.cbz",
                Creation = DateTime.UtcNow,
                LastModification = DateTime.UtcNow
            },

            new()
            {
                Id = 3,
                SerieId = 2,
                FileName = "chapter1.cbz",
                Number = 1,
                Pages = 25,
                Path = "/test_path/serie2/chapter1.cbz",
                Creation = DateTime.UtcNow,
                LastModification = DateTime.UtcNow
            }
        ];

        string savePointPath = Path.Combine(Directory.GetCurrentDirectory(), "savepoint");
        if (Directory.Exists(savePointPath)) Directory.Delete(savePointPath, true);
        Directory.CreateDirectory(savePointPath);
        List<SavePoint> savePoints =
        [
            new()
            {
                Id = 1,
                Path = savePointPath,
                Creation = DateTime.UtcNow
            }
        ];

        List<User> users =
        [
            new()
            {
                Id = 1,
                Username = "TestUser1",
                Role = UserRole.User,
                Creation = DateTime.UtcNow
            },
            new()
            {
                Id = 2,
                Username = "TestAdmin",
                Role = UserRole.Admin,
                Creation = DateTime.UtcNow
            }
        ];

        List<Rank> ranks =
        [
            new()
            {
                Id = 1,
                Value = 1,
                Name = "Terrible"
            },
            new()
            {
                Id = 2,
                Value = 5,
                Name = "Average"
            },
            new()
            {
                Id = 3,
                Value = 10,
                Name = "Excellent"
            }
        ];

        List<UserRank> userRanks =
        [
            new()
            {
                UserId = 1,
                SerieId = 1,
                RankId = 2
            },
            new()
            {
                UserId = 1,
                SerieId = 2,
                RankId = 3
            }
        ];

        ManaxContext context = new(options);

        if (File.Exists(dbPath))
        {
            File.Delete(dbPath);
        }

        context.Database.Migrate();

        context.Libraries.AddRange(libraries);
        context.Series.AddRange(series);
        context.Chapters.AddRange(chapters);
        context.SavePoints.AddRange(savePoints);
        context.Users.AddRange(users);
        context.Ranks.AddRange(ranks);
        context.UserRanks.AddRange(userRanks);

        context.SaveChanges();

        return context;
    }

    public static void CleanupTestDatabase(ManaxContext context)
    {
        if (context.Database.GetDbConnection().ConnectionString.Contains("test_"))
        {
            string? connectionString = context.Database.GetDbConnection().ConnectionString;
            context.Dispose();
            
            if (connectionString != null)
            {
                string dbPath = connectionString.Replace("Data Source=", "");
                if (File.Exists(dbPath))
                {
                    try
                    {
                        File.Delete(dbPath);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }
        }
    }
}