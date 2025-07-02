using System.IO.Compression;
using Microsoft.AspNetCore.Mvc;
using ManaxApi.Auth;
using ManaxApi.Models;
using ManaxLibrary.DTOs.User;

namespace ManaxApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UploadController(ManaxContext context) : ControllerBase
{
    [HttpPost("serie")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadSerie(IFormFile file, [FromForm] long libraryId)
    {
        if (context.Libraries.FirstOrDefault(l => l.Id == libraryId) == null)
            return BadRequest("The library does not exist");
        
        if (!file.FileName.EndsWith(".zip"))
            return BadRequest("The file must be a zip");
        try
        {
            using ZipArchive zipArchive = new(file.OpenReadStream());
            foreach (ZipArchiveEntry entry in zipArchive.Entries)
            {
                Console.WriteLine(entry.FullName);
            }
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest("Invalid zip file");
        }
    }

    // POST: api/upload/chapter
    [HttpPost("chapter")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadChapter(IFormFile file, [FromForm] long serieId)
    {
        return NotFound();
    }

    // POST: api/upload/chapter/replace
    [HttpPost("chapter/replace")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReplaceChapter(IFormFile file, [FromForm] long serieId)
    {
        return NotFound();
    }
}

