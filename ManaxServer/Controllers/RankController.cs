using ManaxLibrary.DTO.Rank;
using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Models.Rank;
using ManaxServer.Services.Mapper;
using ManaxServer.Services.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/rank")]
[ApiController]
public class RankController(ManaxContext context, IMapper mapper, INotificationService notificationService) : ControllerBase
{
    // GET: api/rank
    [HttpGet("/api/ranks")]
    [Authorize(Roles = "User,Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RankDto>>> GetRanks()
    {
        return await context.Ranks.Select(r => mapper.Map<RankDto>(r)).ToListAsync();
    }

    // POST: api/rank
    [HttpPost]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<long>> CreateRank(RankCreateDto rankCreate)
    {
        Rank rank = mapper.Map<Rank>(rankCreate);
        context.Ranks.Add(rank);
        await context.SaveChangesAsync();
        notificationService.NotifyRankCreatedAsync(mapper.Map<RankDto>(rank));
        return rank.Id;
    }

    // PUT: api/rank/5
    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateRank(long id, Rank rank)
    {
        if (id != rank.Id) return BadRequest(Localizer.Format("RankIdMismatch"));
        Rank? found = context.Ranks.FirstOrDefault(r => r.Id == rank.Id);
        if (found == null) return NotFound(Localizer.Format("RankNotFound", id));
        found.Name = rank.Name;
        found.Value = rank.Value;
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

    // DELETE: api/rank/5
    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRank(long id)
    {
        Rank? rank = await context.Ranks.FindAsync(id);
        if (rank == null) return NotFound(Localizer.Format("RankNotFound", id));
        context.Ranks.Remove(rank);
        await context.SaveChangesAsync();
        notificationService.NotifyRankDeletedAsync(id);
        return Ok();
    }

    [HttpPost("set")]
    [Authorize(Roles = "User,Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SetUserRank(UserRankCreateDto rank)
    {
        long? userId = UserController.GetCurrentUserId(HttpContext);
        if (userId == null) return Unauthorized(Localizer.Format("MustBeLoggedInSetRank"));

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

    // GET: api/rank
    [HttpGet("/api/ranking")]
    [Authorize(Roles = "User,Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<UserRankDto>>> GetRanking()
    {
        long? currentUserId = UserController.GetCurrentUserId(HttpContext);
        if (currentUserId == null) return Unauthorized(Localizer.Format("MustBeLoggedInGetRanking"));
        return await context.UserRanks
            .Where(r => r.UserId == currentUserId)
            .Select(r => mapper.Map<UserRankDto>(r)).ToListAsync();
    }
}