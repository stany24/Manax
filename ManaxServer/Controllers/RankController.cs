using ManaxLibrary.DTO.Feature;
using ManaxLibrary.DTO.Rank;
using ManaxLibrary.DTO.User;
using ManaxServer.Attributes;
using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Models.Rank;
using ManaxServer.Services.Feature;
using ManaxServer.Services.Mapper;
using ManaxServer.Services.Notification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/rank")]
[ApiController]
public class RankController(ManaxContext context, IMapper mapper, INotificationService notificationService,IFeatureService featureService)
    : ControllerBase
{
    [HttpGet("/api/ranks")]
    [RequirePermission(Permission.ReadRanks)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RankDto>>> GetRanks()
    {
        if (!featureService.IsFeatureEnabled(FeatureType.Ranks)) { return BadRequest(Localizer.FeatureDisabled(FeatureType.Ranks)); }

        return await context.Ranks.Select(r => mapper.Map<RankDto>(r)).ToListAsync();
    }

    [HttpPost]
    [RequirePermission(Permission.WriteRanks)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<long>> CreateRank(RankCreateDto rankCreate)
    {
        if (!featureService.IsFeatureEnabled(FeatureType.Ranks)) { return BadRequest(Localizer.FeatureDisabled(FeatureType.Ranks)); }

        Rank rank = mapper.Map<Rank>(rankCreate);
        context.Ranks.Add(rank);
        await context.SaveChangesAsync();
        notificationService.NotifyRankCreatedAsync(mapper.Map<RankDto>(rank));
        return rank.Id;
    }

    [HttpPut]
    [RequirePermission(Permission.WriteRanks)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateRank(RankUpdateDto rank)
    {
        if (!featureService.IsFeatureEnabled(FeatureType.Ranks)) { return BadRequest(Localizer.FeatureDisabled(FeatureType.Ranks)); }

        Rank? found = context.Ranks.FirstOrDefault(r => r.Id == rank.Id);
        if (found == null) return NotFound(Localizer.RankNotFound(rank.Id));
        mapper.Map(rank, found);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            return BadRequest(e.Message);
        }

        notificationService.NotifyRankUpdatedAsync(mapper.Map<RankDto>(found));
        return Ok();
    }

    [HttpDelete("{id:long}")]
    [RequirePermission(Permission.DeleteRanks)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRank(long id)
    {
        if (!featureService.IsFeatureEnabled(FeatureType.Ranks)) { return BadRequest(Localizer.FeatureDisabled(FeatureType.Ranks)); }
        
        Rank? rank = await context.Ranks.FindAsync(id);
        if (rank == null) return NotFound(Localizer.RankNotFound(id));
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
        if (!featureService.IsFeatureEnabled(FeatureType.Ranks)) { return BadRequest(Localizer.FeatureDisabled(FeatureType.Ranks)); }

        long? userId = UserController.GetCurrentUserId(HttpContext);
        if (userId == null) return Unauthorized(Localizer.MustBeLoggedInSetRank());

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
        if (!featureService.IsFeatureEnabled(FeatureType.Ranks)) { return BadRequest(Localizer.FeatureDisabled(FeatureType.Ranks)); }

        long? currentUserId = UserController.GetCurrentUserId(HttpContext);
        if (currentUserId == null) return Unauthorized(Localizer.MustBeLoggedInGetRanking());
        return await context.UserRanks
            .Where(r => r.UserId == currentUserId)
            .Select(r => mapper.Map<UserRankDto>(r)).ToListAsync();
    }
}