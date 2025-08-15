using ManaxLibrary.DTO.Rank;
using ManaxLibrary.DTO.Stats;
using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Models.Library;
using ManaxServer.Models.Rank;
using ManaxServer.Models.User;
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserStats))]
    public async Task<ActionResult<UserStats>> GetStats()
    {
        long? currentUserId = UserController.GetCurrentUserId(HttpContext);
        if (currentUserId == null)
        {
            return Unauthorized(Localizer.Format("Unauthorized"));
        }
        
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId.Value);
        if (user == null)
        {
            return NotFound(Localizer.Format("UserNotFound", currentUserId.Value));
        }
        
        List<long> chaptersRead = context.Reads
            .Where(r => r.UserId == currentUserId.Value)
            .Select(r => r.ChapterId)
            .ToList();

        List<RankCount> ranks = context.UserRanks
            .Where(r => r.UserId == currentUserId.Value)
            .GroupBy(r => r.Rank)
            .Select(rankPair => new RankCount { Rank = mapper.Map<RankDto>(rankPair.Key), Count = rankPair.Count() })
            .ToList();


        UserStats stats = new()
        {
            SeriesTotal = await context.Series.CountAsync(),
            ChaptersTotal = await context.Chapters.CountAsync(),
            ChaptersRead = chaptersRead.Count,
            SeriesRead = await context.Series.Where(s => chaptersRead.Contains(s.Id)).CountAsync(),
            Ranks = ranks
        };

        return stats;
    }
    
    [HttpGet("/api/server-stats")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServerStats))]
    public async Task<ActionResult<ServerStats>> GetServerStats()
    {
        long diskSize = 0;
        await foreach (Library library in context.Libraries)
        {
            if (!Directory.Exists(library.Path)) { continue; }
            
            DirectoryInfo dirInfo = new(library.Path);
            diskSize += GetDirectorySize(dirInfo);
        }
        
        DateTime recently = DateTime.UtcNow.AddDays(-7);
        
        ServerStats stats = new()
        {
            DiskSize = diskSize,
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