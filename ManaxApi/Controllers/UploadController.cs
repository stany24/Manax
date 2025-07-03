using System.IO.Compression;
using System.Text.RegularExpressions;
using ImageMagick;
using ManaxApi.Auth;
using ManaxApi.Models;
using ManaxApi.Models.Chapter;
using ManaxApi.Models.Serie;
using ManaxLibrary.DTOs.User;
using Microsoft.AspNetCore.Mvc;

namespace ManaxApi.Controllers;

[Route("api/upload")]
[ApiController]
public partial class UploadController(ManaxContext context) : ControllerBase
{
    [GeneratedRegex("\\d{1,4}")]
    private static partial Regex RegexNumber();
    
    // POST: api/upload/chapter
    [HttpPost("chapter")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadChapter(IFormFile file, [FromForm] long serieId)
    {
        return await UpdateOrReplaceChapter(file,serieId, false);
    }

    // POST: api/upload/chapter/replace
    [HttpPost("chapter/replace")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReplaceChapter(IFormFile file, [FromForm] long serieId)
    {
        return await UpdateOrReplaceChapter(file,serieId, true);
    }
    
    // POST: api/upload/poster
    [HttpPost("poster")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadPoster(IFormFile file, [FromForm] long serieId)
    {
        return await UpdateOrReplacePoster(file,serieId, false);
    }

    // POST: api/upload/poster/replace
    [HttpPost("poster/replace")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReplacePoster(IFormFile file, [FromForm] long serieId)
    {
        return await UpdateOrReplacePoster(file,serieId, true);
    }

    private async Task<IActionResult> UpdateOrReplaceChapter(IFormFile file, [FromForm] long serieId, bool replace)
    {
        Serie? serie = context.Series.FirstOrDefault(s => s.Id == serieId);
        if (serie == null)
            return BadRequest("The serie does not exist");
        
        int pagesCount;
        try
        {
            using ZipArchive zipArchive = new(file.OpenReadStream());
            pagesCount = zipArchive.Entries.Count;
        }
        catch (Exception) { return BadRequest("Invalid zip file"); }
        
        string filePath = Path.Combine(serie.Path, file.FileName);
        if (Directory.Exists(filePath) || System.IO.File.Exists(filePath))
        {
            if (replace) { System.IO.File.Delete(filePath); }
            else { return BadRequest("The chapter already exists"); }
        }
            
        byte[] buffer = new byte[file.Length];
        _ = await file.OpenReadStream().ReadAsync(buffer.AsMemory(0, (int)file.Length));
        await System.IO.File.WriteAllBytesAsync(filePath, buffer);

        int number = 0;
        Regex regex = RegexNumber();
        Match match = regex.Match(file.FileName);
        if (match.Success)
        {
            number = Convert.ToInt32(match.Value);
        }
        
        context.Chapters.Add(new Chapter
        {
            SerieId = serieId,
            Path = filePath,
            FileName = file.FileName,
            Number = number,
            Pages = pagesCount 
        });
        
        await context.SaveChangesAsync();
        return Ok();
    }
    
    private async Task<IActionResult> UpdateOrReplacePoster(IFormFile file, [FromForm] long serieId, bool replace)
    {
        Serie? serie = context.Series.FirstOrDefault(s => s.Id == serieId);
        if (serie == null)
            return BadRequest("The serie does not exist");

        try
        {
            MagickImage image = new(file.OpenReadStream());
            image.Quality = 94;
            await image.WriteAsync(Path.Combine(serie.Path, "poster.webp"), MagickFormat.WebP);
        }
        catch (Exception e)
        {
            return BadRequest($"Invalid image file: {e.Message}");
        }
        
        return Ok();
    }
}

