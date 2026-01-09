using ManaxLibrary;
using ManaxLibrary.DTO.Feature;
using ManaxLibrary.DTO.Rank;
using ManaxLibrary.DTO.User;
using ManaxServer.Attributes;
using ManaxServer.Models;
using ManaxServer.Models.Rank;
using ManaxServer.Services.Notification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/rank")]
[ApiController]
public class RankController(
    ManaxContext context,
    INotificationService notificationService)
    : ControllerBase
{
    [HttpGet("/api/ranks")]
    [RequirePermission(Permission.ReadRanks)]
    [RequireFeature(FeatureType.Ranks)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RankDto>>> GetRanks()
    {
        List<RankDto> ranks = await context.Ranks
            .Select(rank => rank.ToDto())
            .ToListAsync();
        return Ok(ranks);
    }

    [HttpPost]
    [RequirePermission(Permission.WriteRanks)]
    [RequireFeature(FeatureType.Ranks)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<long>> CreateRank(RankCreateDto rankCreate)
    {
        if(!rankCreate.IsValid()) return BadRequest(ErrorCode.InvalidRankData);
        
        Rank rank = Rank.Create(rankCreate);
        context.Ranks.Add(rank);
        await context.SaveChangesAsync();
        notificationService.NotifyRankCreatedAsync(rank.ToDto());
        return Ok(rank.Id);
    }

    [HttpPut]
    [RequirePermission(Permission.WriteRanks)]
    [RequireFeature(FeatureType.Ranks)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateRank(RankUpdateDto rankUpdate)
    {
        Rank? rank = context.Ranks.FirstOrDefault(r => r.Id == rankUpdate.Id);
        if (rank == null) return NotFound(ErrorCode.RankDoesNotExist);
        if (!rankUpdate.IsValid()) return BadRequest(ErrorCode.InvalidRankData);

        rank.Update(rankUpdate);
        try { await context.SaveChangesAsync(); }
        catch{ return BadRequest(ErrorCode.InvalidRankData); }
        
        notificationService.NotifyRankUpdatedAsync(rank.ToDto());
        return Ok();
    }

    [HttpDelete("{id:long}")]
    [RequirePermission(Permission.DeleteRanks)]
    [RequireFeature(FeatureType.Ranks)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteRank(long id)
    {
        Rank? rank = await context.Ranks.FindAsync(id);
        if (rank == null) return NotFound(ErrorCode.RankDoesNotExist);
        
        context.Ranks.Remove(rank);
        await context.SaveChangesAsync();
        notificationService.NotifyRankDeletedAsync(id);
        return Ok();
    }

    [HttpPost("set")]
    [RequirePermission(Permission.SetMyRank)]
    [RequireFeature(FeatureType.Ranks)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> SetUserRank(UserRankCreateDto rank)
    {
        long? userId = UserController.GetCurrentUserId(HttpContext);
        if (userId == null) return Unauthorized(ErrorCode.TokenRequired);

        UserRank? existing =
            await context.UserRanks.FirstOrDefaultAsync(ur => ur.UserId == userId.Value && ur.SerieId == rank.SerieId);
        if (existing != null)
        {
            existing.RankId = rank.RankId;
        }
        else
        {
            UserRank userRank = new()
            {
                UserId = userId.Value,
                SerieId = rank.SerieId,
                RankId = rank.RankId
            };
            context.UserRanks.Add(userRank);
        }

        await context.SaveChangesAsync();
        return Created();
    }

    [HttpGet("/api/ranking")]
    [RequirePermission(Permission.ReadRanks)]
    [RequireFeature(FeatureType.Ranks)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<UserRankDto>>> GetRanking()
    {
        long? currentUserId = UserController.GetCurrentUserId(HttpContext);
        if (currentUserId == null) return Unauthorized(ErrorCode.TokenRequired);
        List<UserRankDto> userRanks = await context.UserRanks
            .Where(r => r.UserId == currentUserId)
            .Select(r => r.ToDto())
            .ToListAsync();
        return Ok(userRanks);
    }
}