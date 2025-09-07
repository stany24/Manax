using ManaxLibrary.DTO.Serie;
using ManaxServer.Models.Serie;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.SerieTests;

[TestClass]
public class PutSerieTests: SerieTestsSetup
{
    [TestMethod]
    public async Task PutSerie_WithValidId_UpdatesSerie()
    {
        Serie serie = _context.Series.First();
        SerieUpdateDto updateDto = new()
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Status = Status.Completed
        };

        IActionResult result = await _controller.PutSerie(serie.Id, updateDto);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Serie? updatedSerie = await _context.Series.FindAsync(serie.Id);
        Assert.IsNotNull(updatedSerie);
        Assert.AreEqual(updateDto.Title, updatedSerie.Title);
        Assert.AreEqual(updateDto.Description, updatedSerie.Description);
        Assert.AreEqual(updateDto.Status, updatedSerie.Status);
    }

    [TestMethod]
    public async Task PutSerie_WithInvalidId_ReturnsNotFound()
    {
        SerieUpdateDto updateDto = new()
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Status = Status.Completed
        };

        IActionResult result = await _controller.PutSerie(999999, updateDto);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task PutSerie_WithEmptyTitle_ReturnsBadRequest()
    {
        Serie serie = _context.Series.First();
        SerieUpdateDto updateDto = new()
        {
            Title = "",
            Description = "Updated Description",
            Status = Status.Completed
        };

        IActionResult result = await _controller.PutSerie(serie.Id, updateDto);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task PutSerie_VerifyLastModificationUpdated()
    {
        Serie serie = _context.Series.First();
        DateTime originalLastModification = serie.LastModification;

        SerieUpdateDto updateDto = new()
        {
            Title = "Updated Title with Time Check",
            Description = "Updated Description",
            Status = Status.Completed
        };

        await Task.Delay(10);

        IActionResult result = await _controller.PutSerie(serie.Id, updateDto);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Serie? updatedSerie = await _context.Series.FindAsync(serie.Id);
        Assert.IsNotNull(updatedSerie);
        Assert.IsTrue(updatedSerie.LastModification < DateTime.UtcNow);
        Assert.IsTrue(updatedSerie.LastModification > originalLastModification);
    }
}