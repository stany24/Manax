using AutoMapper;
using ManaxLibrary.DTOs.Rank;
using ManaxLibrary.DTOs.User;
using ManaxServer.Auth;
using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Models.Rank;
using ManaxServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/rank")]
[ApiController]
public class RankController(ManaxContext context, IMapper mapper) : ControllerBase
{
    // GET: api/rank
    [HttpGet("/api/ranks")]
    [AuthorizeRole(UserRole.User)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RankDTO>))]
    public async Task<ActionResult<IEnumerable<RankDTO>>> GetRanks()
    {
        return await context.Ranks.Select(r => mapper.Map<RankDTO>(r)).ToListAsync();
    }

    // POST: api/rank
    [HttpPost]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(long))]
    public async Task<ActionResult<long>> CreateRank(RankCreateDTO rankCreate)
    {
        Rank rank = mapper.Map<Rank>(rankCreate);
        context.Ranks.Add(rank);
        await context.SaveChangesAsync();
        NotificationService.NotifyRankCreatedAsync(mapper.Map<RankDTO>(rank));
        return rank.Id;
    }

    // PUT: api/rank/5
    [HttpPut("{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
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
        NotificationService.NotifyRankUpdatedAsync(mapper.Map<RankDTO>(found));
        return NoContent();
    }

    // DELETE: api/rank/5
    [HttpDelete("{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRank(long id)
    {
        Rank? rank = await context.Ranks.FindAsync(id);
        if (rank == null) return NotFound(Localizer.Format("RankNotFound", id));
        context.Ranks.Remove(rank);
        await context.SaveChangesAsync();
        NotificationService.NotifyRankDeletedAsync(id);
        return NoContent();
    }

    [HttpPost("set")]
    [AuthorizeRole(UserRole.User)]
    public async Task<IActionResult> SetUserRank(UserRankCreateDTO rank)
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
        return NoContent();
    }

    // GET: api/rank
    [HttpGet("/api/ranking")]
    [AuthorizeRole(UserRole.User)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserRankDTO>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<UserRankDTO>>> GetRanking()
    {
        long? currentUserId = UserController.GetCurrentUserId(HttpContext);
        if (currentUserId == null) return Unauthorized(Localizer.Format("MustBeLoggedInGetRanking"));
        return await context.UserRanks
            .Where(r => r.UserId == currentUserId)
            .Select(r => mapper.Map<UserRankDTO>(r)).ToListAsync();
    }
}