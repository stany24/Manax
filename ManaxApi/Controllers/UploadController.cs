using System.IO.Compression;
using ManaxApi.Auth;
using ManaxApi.Models;
using ManaxApi.Models.Library;
using ManaxLibrary.DTOs.User;
using Microsoft.AspNetCore.Mvc;

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
        Library? library = context.Libraries.FirstOrDefault(l => l.Id == libraryId);
        if (library == null)
            return BadRequest("The library does not exist");
        
        try
        {
            using ZipArchive zipArchive = new(file.OpenReadStream());
            string folderPath = Path.Combine(library.Path, file.FileName);
            if(Directory.Exists(folderPath) || System.IO.File.Exists(folderPath))
            {
                return BadRequest("The serie already exists");
            }
            Directory.CreateDirectory(folderPath);
            zipArchive.ExtractToDirectory(folderPath);
            return Ok();
        }
        catch (Exception)
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

