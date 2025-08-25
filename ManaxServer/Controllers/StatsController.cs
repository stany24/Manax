using ManaxLibrary.DTO.Rank;
using ManaxLibrary.DTO.Read;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.DTO.Stats;
using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Models.SavePoint;
using ManaxServer.Services.Mapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/chapter")]
[ApiController]
public class StatsController(ManaxContext context, IMapper mapper) : ControllerBase
{
    // GET: api/Chapter
    [HttpGet("/api/stats")]
    [Authorize(Roles = "User,Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserStats>> GetStats()
    {
        long? currentUserId = UserController.GetCurrentUserId(HttpContext);
        if (currentUserId == null) return Unauthorized(Localizer.Format("Unauthorized"));
        
        List<long> chaptersRead = context.Reads
            .Where(r => r.UserId == currentUserId.Value)
            .Select(r => r.ChapterId)
            .ToList();

        List<RankCount> ranks = context.UserRanks
            .Where(r => r.UserId == currentUserId.Value)
            .GroupBy(r => r.Rank)
            .Select(rankPair => new RankCount { Rank = mapper.Map<RankDto>(rankPair.Key), Count = rankPair.Count() })
            .ToList();

        List<ReadDto> reads = context.Reads
            .Where(r => r.UserId == currentUserId.Value)
            .Select(r => mapper.Map<ReadDto>(r))
            .ToList();

        int seriesCompleted = await context.Series
            .Where(s => context.Chapters.Any(c => c.SerieId == s.Id))
            .Where(s => context.Chapters
                .Where(c => c.SerieId == s.Id)
                .All(c => chaptersRead.Contains(c.Id)))
            .CountAsync();

        UserStats stats = new()
        {
            SeriesTotal = await context.Series.CountAsync(),
            ChaptersTotal = await context.Chapters.CountAsync(),
            ChaptersRead = chaptersRead.Count,
            SeriesInProgress = await context.Series.Where(s => chaptersRead.Contains(s.Id)).CountAsync() - seriesCompleted,
            SeriesCompleted = seriesCompleted,
            Ranks = ranks,
            Reads = reads
        };

        return stats;
    }

    [HttpGet("/api/server-stats")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ServerStats>> GetServerStats()
    {
        long diskSize = 0;
        long availableDiskSize = 0;
        await foreach (SavePoint savePoint in context.SavePoints)
        {
            if (!Directory.Exists(savePoint.Path)) continue;

            DirectoryInfo dirInfo = new(savePoint.Path);
            diskSize += GetDirectorySize(dirInfo);
            availableDiskSize += new DriveInfo(Path.GetPathRoot(savePoint.Path) ?? "/").AvailableFreeSpace;
        }

        DateTime recently = DateTime.UtcNow.AddDays(-7);

        Dictionary<string, int> seriesInLibraries = await context.Series
            .GroupBy(s => s.Library != null ? s.Library.Name : "No library")
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        List<SerieDto> neverReadSeries = await context.Series
            .Where(s => !context.Reads.Any(r => context.Chapters.Any(c => c.SerieId == s.Id && c.Id == r.ChapterId)))
            .Select(s => mapper.Map<SerieDto>(s))
            .ToListAsync();
        
        ServerStats stats = new()
        {
            SeriesInLibraries = seriesInLibraries,
            DiskSize = diskSize,
            AvailableDiskSize = availableDiskSize,
            NeverReadSeries = neverReadSeries,
            Series = await context.Series.CountAsync(),
            Chapters = await context.Chapters.CountAsync(),
            Users = await context.Users.CountAsync(),
            ActiveUsers = await context.Users.Where(u => u.LastLogin > recently).CountAsync()
        };

        return stats;
    }

    private static long GetDirectorySize(DirectoryInfo d)
    {
        FileInfo[] fis = d.GetFiles();
        long size = fis.Sum(fi => fi.Length);
        DirectoryInfo[] dis = d.GetDirectories();
        size += dis.Sum(GetDirectorySize);
        return size;
    }
}