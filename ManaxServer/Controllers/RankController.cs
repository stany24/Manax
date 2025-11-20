using ManaxLibrary.DTO.Feature;
using ManaxLibrary.DTO.Rank;
using ManaxLibrary.DTO.User;
using ManaxServer.Attributes;
using ManaxServer.Models;
using ManaxServer.Models.Rank;
using ManaxServer.Services.Feature;
using ManaxServer.Services.Notification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/rank")]
[ApiController]
public class RankController(
    ManaxContext context,
    INotificationService notificationService,
    IFeatureService featureService)
    : ControllerBase
{
    [HttpGet("/api/ranks")]
    [RequirePermission(Permission.ReadRanks)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RankDto>>> GetRanks()
    {
        if (!featureService.IsFeatureEnabled(FeatureType.Ranks))
            return BadRequest();

        return await context.Ranks.Select(rank => rank.ToDto()).ToListAsync();
    }

    [HttpPost]
    [RequirePermission(Permission.WriteRanks)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<long>> CreateRank(RankCreateDto rankCreate)
    {
        if (!featureService.IsFeatureEnabled(FeatureType.Ranks))
            return BadRequest();

        Rank rank = Rank.Create(rankCreate);
        context.Ranks.Add(rank);
        await context.SaveChangesAsync();
        notificationService.NotifyRankCreatedAsync(rank.ToDto());
        return rank.Id;
    }

    [HttpPut]
    [RequirePermission(Permission.WriteRanks)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateRank(RankUpdateDto rankUpdate)
    {
        if (!featureService.IsFeatureEnabled(FeatureType.Ranks))
            return BadRequest();

        Rank? rank = context.Ranks.FirstOrDefault(r => r.Id == rankUpdate.Id);
        if (rank == null) return NotFound();
        rank.Update(rankUpdate);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            return BadRequest(e.Message);
        }

        notificationService.NotifyRankUpdatedAsync(rank.ToDto());
        return Ok();
    }

    [HttpDelete("{id:long}")]
    [RequirePermission(Permission.DeleteRanks)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRank(long id)
    {
        if (!featureService.IsFeatureEnabled(FeatureType.Ranks))
            return BadRequest();

        Rank? rank = await context.Ranks.FindAsync(id);
        if (rank == null) return NotFound();
        context.Ranks.Remove(rank);
        await context.SaveChangesAsync();
        notificationService.NotifyRankDeletedAsync(id);
        return Ok();
    }

    [HttpPost("set")]
    [RequirePermission(Permission.SetMyRank)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SetUserRank(UserRankCreateDto rank)
    {
        if (!featureService.IsFeatureEnabled(FeatureType.Ranks))
            return BadRequest();

        long? userId = UserController.GetCurrentUserId(HttpContext);
        if (userId == null) return Unauthorized();

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
        return Ok();
    }

    [HttpGet("/api/ranking")]
    [RequirePermission(Permission.ReadRanks)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<UserRankDto>>> GetRanking()
    {
        if (!featureService.IsFeatureEnabled(FeatureType.Ranks))
            return BadRequest();

        long? currentUserId = UserController.GetCurrentUserId(HttpContext);
        if (currentUserId == null) return Unauthorized();
        return await context.UserRanks
            .Where(r => r.UserId == currentUserId)
            .Select(r => r.ToDto()).ToListAsync();
    }
}