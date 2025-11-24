using ManaxLibrary.DTO.SavePoint;
using ManaxLibrary.DTO.User;
using ManaxLibrary.Logging;
using ManaxServer.Attributes;
using ManaxServer.Models;
using ManaxServer.Models.SavePoint;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/save-point")]
[ApiController]
public class SavePointController(ManaxContext context) : ControllerBase
{
    [HttpPost("create")]
    [RequirePermission(Permission.WriteSavePoints)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<long>> PostSavePoint(SavePointCreateDto savePointCreate)
    {
        if (await context.SavePoints.AnyAsync(l => l.Path == savePointCreate.Path) ||
            !Directory.Exists(savePointCreate.Path))
            return Conflict();

        SavePoint savePoint = SavePoint.Create(savePointCreate);
        context.SavePoints.Add(savePoint);

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("constraint") ?? false)
        {
            return Conflict();
        }

        Logger.LogInfo("Created new save point with ID " + savePoint.Id + " at: " + savePoint.Path);
        return savePoint.Id;
    }
}