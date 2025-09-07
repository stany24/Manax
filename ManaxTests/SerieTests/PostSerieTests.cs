using ManaxLibrary.DTO.Serie;
using ManaxServer.Models.Serie;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.SerieTests;

[TestClass]
public class PostSerieTests : SerieTestsSetup
{
    [TestMethod]
    public async Task PostSerie_WithValidData_CreatesSerie()
    {
        SerieCreateDto createDto = new()
        {
            Title = "New Serie"
        };

        ActionResult<long> result = await Controller.PostSerie(createDto);

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        long? serieId = result.Value;
        Assert.IsNotNull(serieId);

        Serie? createdSerie = await Context.Series.FindAsync(serieId);
        Assert.IsNotNull(createdSerie);
        Assert.AreEqual(createDto.Title, createdSerie.Title);
    }

    [TestMethod]
    public async Task PostSerie_WithEmptyTitle_ReturnsBadRequest()
    {
        SerieCreateDto createDto = new()
        {
            Title = ""
        };

        ActionResult<long> result = await Controller.PostSerie(createDto);

        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task PostSerie_CreationDateSetCorrectly()
    {
        SerieCreateDto createDto = new()
        {
            Title = "New Serie with Date"
        };

        DateTime beforeCreation = DateTime.UtcNow;
        ActionResult<long> result = await Controller.PostSerie(createDto);
        DateTime afterCreation = DateTime.UtcNow;

        long? serieId = result.Value;
        Assert.IsNotNull(serieId);

        Serie? createdSerie = await Context.Series.FindAsync(serieId);
        Assert.IsNotNull(createdSerie);
        Assert.IsTrue(createdSerie.Creation >= beforeCreation);
        Assert.IsTrue(createdSerie.Creation <= afterCreation);
    }
}