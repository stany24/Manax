using ManaxLibrary.DTO.Tag;
using ManaxLibrary.DTO.User;
using ManaxServer.Attributes;
using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Models.Tag;
using ManaxServer.Services.Notification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/tag")]
[ApiController]
public class TagController(ManaxContext context, INotificationService notificationService)
    : ControllerBase
{
    [HttpGet("/api/tags")]
    [RequirePermission(Permission.ReadTags)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TagDto>>> GetTags()
    {
        return await context.Tags.Select(t => t.ToDto()).ToListAsync();
    }

    [HttpPost]
    [RequirePermission(Permission.WriteTags)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateTag(TagCreateDto tagCreate)
    {
        Tag tag = Tag.Create(tagCreate);
        context.Tags.Add(tag);
        await context.SaveChangesAsync();
        notificationService.NotifyTagCreatedAsync(tag.ToDto());
        return Ok();
    }

    [HttpPut]
    [RequirePermission(Permission.WriteTags)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateTag(TagUpdateDto tagUpdate)
    {
        Tag? tag = context.Tags.FirstOrDefault(r => r.Id == tagUpdate.Id);
        if (tag == null) return NotFound(Localizer.TagNotFound(tagUpdate.Id));
        tag.Update(tagUpdate);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            return BadRequest(e.Message);
        }

        notificationService.NotifyTagUpdatedAsync(tag.ToDto());
        return Ok();
    }

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
}