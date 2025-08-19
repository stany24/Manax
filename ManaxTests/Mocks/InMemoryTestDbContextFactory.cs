using ManaxLibrary.DTO.Serie;
using ManaxServer.Models;
using ManaxServer.Models.Chapter;
using ManaxServer.Models.SavePoint;
using ManaxServer.Models.Serie;
using Microsoft.EntityFrameworkCore;

namespace ManaxTests.Mocks;

public static class InMemoryTestDbContextFactory
{
    public static ManaxContext CreateTestContext(string? dbName = null)
    {
        dbName ??= Guid.NewGuid().ToString();

        DbContextOptions<ManaxContext> options = new DbContextOptionsBuilder<ManaxContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

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
                Creation = DateTime.Now,
                LastModification = DateTime.Now
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
                Creation = DateTime.Now,
                LastModification = DateTime.Now
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
                Creation = DateTime.Now,
                LastModification = DateTime.Now
            },

            new()
            {
                Id = 2,
                SerieId = 1,
                FileName = "chapter2.cbz",
                Number = 2,
                Pages = 30,
                Path = "/test_path/serie1/chapter2.cbz",
                Creation = DateTime.Now,
                LastModification = DateTime.Now
            },

            new()
            {
                Id = 3,
                SerieId = 2,
                FileName = "chapter1.cbz",
                Number = 1,
                Pages = 25,
                Path = "/test_path/serie2/chapter1.cbz",
                Creation = DateTime.Now,
                LastModification = DateTime.Now
            }
        ];
        
        List<SavePoint> savePoints =
        [
            new()
            {
                Id = 1,
                Path = "/test_path/",
                Creation = DateTime.Now
            }
        ];

        ManaxContext context = new(options);

        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        context.Series.AddRange(series);
        context.Chapters.AddRange(chapters);

        context.SaveChanges();

        return context;
    }
}