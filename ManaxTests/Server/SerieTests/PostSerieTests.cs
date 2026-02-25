using ManaxLibrary;
using ManaxLibrary.DTO.Serie;
using ManaxServer.Models.Serie;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.Server.SerieTests;

[TestClass]
public class PostSerieTests : SerieTestsSetup
{
    [TestMethod]
    public async Task PostSerieWithValidDataCreatesSerie()
    {
        SerieCreateDto createDto = new()
        {
            Title = "New Serie"
        };

        ActionResult<long> result = await Controller.PostSerie(createDto);

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        long? serieId = okResult.Value as long?;
        Assert.IsNotNull(serieId);

        Serie? createdSerie = await Context.Series.FindAsync(serieId);
        Assert.IsNotNull(createdSerie);
        Assert.AreEqual(createDto.Title, createdSerie.Title);
    }

    [TestMethod]
    public async Task PostSerieWithEmptyTitleReturnsBadRequest()
    {
        SerieCreateDto createDto = new()
        {
            Title = ""
        };

        ActionResult<long> result = await Controller.PostSerie(createDto);

        TestSetup.CheckTypeAndErrorCode<BadRequestObjectResult>(result.Result, ErrorCode.InvalidSerieData);
    }

    [TestMethod]
    public async Task PostSerieCreationDateSetCorrectly()
    {
        SerieCreateDto createDto = new()
        {
            Title = "New Serie with Date"
        };

        DateTime beforeCreation = DateTime.UtcNow;
        ActionResult<long> result = await Controller.PostSerie(createDto);
        DateTime afterCreation = DateTime.UtcNow;

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        long? serieId = okResult.Value as long?;
        Assert.IsNotNull(serieId);

        Serie? createdSerie = await Context.Series.FindAsync(serieId);
        Assert.IsNotNull(createdSerie);
        Assert.IsTrue(createdSerie.Creation >= beforeCreation);
        Assert.IsTrue(createdSerie.Creation <= afterCreation);
    }
}