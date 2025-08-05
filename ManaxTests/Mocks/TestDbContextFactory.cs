using System.Reflection;
using AutoMapper;
using ManaxLibrary.DTOs.Chapter;
using ManaxLibrary.DTOs.Serie;
using ManaxServer.Models;
using ManaxServer.Models.Chapter;
using ManaxServer.Models.Serie;
using Microsoft.EntityFrameworkCore;
using Moq;
using Status = ManaxLibrary.DTOs.Serie.Status;

namespace ManaxTests.Mocks;

public static class TestDbContextFactory
{
    public static (Mock<ManaxContext> Context, List<Chapter> Chapters, List<Serie> Series) CreateMockContext()
    {
        List<Serie> series =
        [
            new()
            {
                Id = 1,
                LibraryId = 1,
                FolderName = "serie1",
                Title = "Serie 1",
                Description = "Description for Serie 1",
                Path = "/test_path/serie1",
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
                Path = "/test_path/serie2",
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

        Mock<DbSet<Serie>> mockSerieDbSet = SetupMockDbSet(series);

        Mock<DbSet<Chapter>> mockChapterDbSet = SetupMockDbSet(chapters);

        Mock<ManaxContext> mockContext = new(new DbContextOptions<ManaxContext>());
        mockContext.Setup(c => c.Series).Returns(mockSerieDbSet.Object);
        mockContext.Setup(c => c.Chapters).Returns(mockChapterDbSet.Object);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        return (mockContext, chapters, series);
    }
    
    public static Mock<IMapper> CreateMockMapper()
    {
        Mock<IMapper> mockMapper = new();
        
        mockMapper.Setup(m => m.Map<ChapterDTO>(It.IsAny<Chapter>()))
            .Returns<Chapter>(chapter => new ChapterDTO
            {
                Id = chapter.Id,
                SerieId = chapter.SerieId,
                FileName = chapter.FileName,
                Number = chapter.Number,
                Pages = chapter.Pages,
                Creation = chapter.Creation,
                LastModification = chapter.LastModification
            });

        mockMapper.Setup(m => m.Map<SerieDTO>(It.IsAny<Serie>()))
            .Returns<Serie>(serie => new SerieDTO
            {
                Id = serie.Id,
                LibraryId = serie.LibraryId,
                Title = serie.Title,
                Description = serie.Description,
                Status = serie.Status,
                Creation = serie.Creation,
                LastModification = serie.LastModification
            });
            
        mockMapper.Setup(m => m.Map(It.IsAny<SerieUpdateDTO>(), It.IsAny<Serie>()))
            .Callback<SerieUpdateDTO, Serie>((updateDto, serie) =>
            {
                serie.Title = updateDto.Title;
                serie.Description = updateDto.Description;
                serie.Status = updateDto.Status;
            });

        return mockMapper;
    }
    
    private static Mock<DbSet<T>> SetupMockDbSet<T>(List<T> data) where T : class
    {
        Mock<DbSet<T>> mockDbSet = new();
        
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.AsQueryable().Provider);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.AsQueryable().Expression);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.AsQueryable().ElementType);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.AsQueryable().GetEnumerator());

        mockDbSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));

        mockDbSet.As<IQueryable<T>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(data.AsQueryable().Provider));

        if (typeof(T) == typeof(Chapter) || typeof(T) == typeof(Serie))
        {
            mockDbSet.Setup(d => d.FindAsync(It.IsAny<object[]>()))
                .Returns<object[]>(ids =>
                {
                    long id = (long)ids[0];
                    T? entity = data.FirstOrDefault(e => GetEntityId(e) == id);
                    ValueTask<T> mockValueTask = new(entity);
                    return mockValueTask;
                });
        }

        mockDbSet.Setup(d => d.Remove(It.IsAny<T>())).Callback<T>((entity) => data.Remove(entity));

        return mockDbSet;
    }
    
    private static long GetEntityId<T>(T entity)
    {
        PropertyInfo? property = entity?.GetType().GetProperty("Id");
        return property != null ? (long)property.GetValue(entity)! : 0;
    }
}
