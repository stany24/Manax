using ManaxLibrary.DTO.Tag;
using ManaxLibrary.DTO.User;
using ManaxServer.Attributes;
using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Models.Serie;
using ManaxServer.Models.Tag;
using ManaxServer.Services.Mapper;
using ManaxServer.Services.Notification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/tag")]
[ApiController]
public class TagController(ManaxContext context, IMapper mapper, INotificationService notificationService)
    : ControllerBase
{
    // GET: api/tag
    [HttpGet("/api/tags")]
    [RequirePermission(Permission.ReadTags)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TagDto>>> GetTags()
    {
        return await context.Tags.Select(r => mapper.Map<TagDto>(r)).ToListAsync();
    }

    // POST: api/tag
    [HttpPost]
    [RequirePermission(Permission.WriteTags)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<long>> CreateTag(TagCreateDto tagCreate)
    {
        Tag tag = mapper.Map<Tag>(tagCreate);
        context.Tags.Add(tag);
        await context.SaveChangesAsync();
        notificationService.NotifyTagCreatedAsync(mapper.Map<TagDto>(tag));
        return tag.Id;
    }

    // PUT: api/tag
    [HttpPut]
    [RequirePermission(Permission.WriteTags)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateTag(Tag tag)
    {
        Tag? found = context.Tags.FirstOrDefault(r => r.Id == tag.Id);
        if (found == null) return NotFound(Localizer.TagNotFound(tag.Id));
        found.Name = tag.Name;
        found.Color = tag.Color;
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            return BadRequest(e.Message);
        }

        notificationService.NotifyTagUpdatedAsync(mapper.Map<TagDto>(found));
        return Ok();
    }

    // DELETE: api/tag/5
    [HttpDelete("{id:long}")]
    [RequirePermission(Permission.DeleteTags)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTag(long id)
    {
        Tag? tag = await context.Tags.FindAsync(id);
        if (tag == null) return NotFound(Localizer.TagNotFound(id));
        context.Tags.Remove(tag);
        await context.SaveChangesAsync();
        notificationService.NotifyTagDeletedAsync(id);
        return Ok();
    }

    [HttpPost("serie")]
    [RequirePermission(Permission.SetSerieTags)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SetSerieTags(TagOfSerieDto info)
    {
        Serie? serie = context.Series.FirstOrDefault(s => s.Id == info.serieId);
        if(serie == null)
            return NotFound(Localizer.SerieNotFound(info.serieId));

        IQueryable<SerieTag> existingTags = context.SerieTags.Where(st => st.SerieId == info.serieId);
        context.SerieTags.RemoveRange(existingTags);
        
        foreach (TagDto tagDto in info.tags)
        {
            Tag? tag = await context.Tags.FirstOrDefaultAsync(t => t.Id == tagDto.Id);
            if (tag == null) continue;
            SerieTag serieTag = new()
            {
                SerieId = info.serieId,
                TagId = tag.Id
            };
            context.SerieTags.Add(serieTag);
        }

        await context.SaveChangesAsync();
        return Ok();
    }
}