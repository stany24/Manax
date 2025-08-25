using ManaxLibrary.DTO.SavePoint;
using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Models.SavePoint;
using ManaxServer.Services.Mapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/save-point")]
[ApiController]
public class SavePointController(ManaxContext context, IMapper mapper) : ControllerBase
{
    // POST: api/SavePoint
    [HttpPost("create")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<long>> PostSavePoint(SavePointCreateDto savePointCreate)
    {
        // Check if name is unique
        if (await context.SavePoints.AnyAsync(l => l.Path == savePointCreate.Path))
            return Conflict(Localizer.Format("SavePointNameExists", savePointCreate.Path));

        if (!Directory.Exists(savePointCreate.Path))
        {
            return Conflict(Localizer.Format("SavePointPathNotExists", savePointCreate.Path));
        }
        
        SavePoint library = mapper.Map<SavePoint>(savePointCreate);
        library.Creation = DateTime.UtcNow;

        context.SavePoints.Add(library);

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("constraint") ?? false)
        {
            return Conflict(Localizer.Format("SavePointNameExists"));
        }

        return library.Id;
    }
}