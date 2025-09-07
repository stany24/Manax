using ManaxLibrary.DTO.Serie;
using ManaxServer.Models.Chapter;
using ManaxServer.Models.Serie;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.SerieTests;

[TestClass]
public class GetSerieTests : SerieTestsSetup
{
    [TestMethod]
    public async Task GetSeries_ReturnsAllSerieIds()
    {
        ActionResult<IEnumerable<long>> result = await Controller.GetSeries();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        List<long>? returnedIds = result.Value as List<long>;
        Assert.IsNotNull(returnedIds);

        Assert.AreEqual(Context.Series.Count(), returnedIds.Count);
        foreach (Serie serie in Context.Series) Assert.Contains(serie.Id, returnedIds);
    }

    [TestMethod]
    public async Task GetSerie_WithValidId_ReturnsSerie()
    {
        Serie serie = Context.Series.First();
        ActionResult<SerieDto> result = await Controller.GetSerie(serie.Id);

        SerieDto? returnedSerie = result.Value;
        Assert.IsNotNull(returnedSerie);
        Assert.AreEqual(serie.Id, returnedSerie.Id);
        Assert.AreEqual(serie.LibraryId, returnedSerie.LibraryId);
        Assert.AreEqual(serie.Title, returnedSerie.Title);
        Assert.AreEqual(serie.Description, returnedSerie.Description);
        Assert.AreEqual(serie.Status, returnedSerie.Status);
    }

    [TestMethod]
    public async Task GetSerie_WithInvalidId_ReturnsNotFound()
    {
        ActionResult<SerieDto> result = await Controller.GetSerie(999999);

        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public void GetSerieChapters_WithValidId_ReturnsChapterIds()
    {
        Serie serie = Context.Series.First();
        List<Chapter> chaptersOfSerie = Context.Chapters.Where(c => c.SerieId == serie.Id).ToList();

        ActionResult<List<long>> result = Controller.GetSerieChapters(serie.Id);

        Assert.IsNull(result.Result);

        Assert.IsNotNull(result.Value);

        List<long> returnedIds = result.Value.ToList();
        Assert.AreEqual(chaptersOfSerie.Count, returnedIds.Count);
        foreach (Chapter chapter in chaptersOfSerie) Assert.Contains(chapter.Id, returnedIds);
    }

    [TestMethod]
    public void GetSerieChapters_WithInvalidId_ReturnsNotFound()
    {
        ActionResult<List<long>> result = Controller.GetSerieChapters(999999);

        NotFoundResult? notFoundResult = result.Result as NotFoundResult;
        Assert.IsNotNull(notFoundResult);

        Assert.IsNull(result.Value);
    }

    [TestMethod]
    public async Task GetSerie_VerifyAllPropertiesMapping()
    {
        Serie serie = Context.Series.First();
        ActionResult<SerieDto> result = await Controller.GetSerie(serie.Id);

        SerieDto? returnedSerie = result.Value;
        Assert.IsNotNull(returnedSerie);
        Assert.AreEqual(serie.Id, returnedSerie.Id);
        Assert.AreEqual(serie.LibraryId, returnedSerie.LibraryId);
        Assert.AreEqual(serie.Title, returnedSerie.Title);
        Assert.AreEqual(serie.Description, returnedSerie.Description);
        Assert.AreEqual(serie.Status, returnedSerie.Status);
        Assert.AreEqual(serie.Creation, returnedSerie.Creation);
        Assert.AreEqual(serie.LastModification, returnedSerie.LastModification);
    }
}