using ManaxLibrary;
using ManaxLibrary.DTO.Tag;
using ManaxLibrary.DTO.User;
using ManaxServer.Attributes;
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
        List<TagDto> tags = await context.Tags
            .Select(t => t.ToDto())
            .ToListAsync();
        return Ok(tags);
    }

    [HttpPost]
    [RequirePermission(Permission.WriteTags)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> CreateTag(TagCreateDto tagCreate)
    {
        Tag tag = Tag.Create(tagCreate);
        context.Tags.Add(tag);
        await context.SaveChangesAsync();
        notificationService.NotifyTagCreatedAsync(tag.ToDto());
        return Ok(tag.Id);
    }

    [HttpPut]
    [RequirePermission(Permission.WriteTags)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateTag(TagUpdateDto tagUpdate)
    {
        Tag? tag = context.Tags.FirstOrDefault(r => r.Id == tagUpdate.Id);
        if (tag == null) return NotFound(ErrorCode.TagDoesNotExist);
        tag.Update(tagUpdate);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return BadRequest(ErrorCode.InvalidTagData);
        }

        notificationService.NotifyTagUpdatedAsync(tag.ToDto());
        return Ok();
    }

    [HttpDelete("{id:long}")]
    [RequirePermission(Permission.DeleteTags)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteTag(long id)
    {
        Tag? tag = await context.Tags.FindAsync(id);
        if (tag == null) return NotFound(ErrorCode.TagDoesNotExist);
        context.Tags.Remove(tag);
        await context.SaveChangesAsync();
        notificationService.NotifyTagDeletedAsync(id);
        return Ok();
    }
}