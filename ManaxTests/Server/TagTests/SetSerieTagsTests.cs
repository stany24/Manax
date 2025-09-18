using ManaxLibrary.DTO.Tag;
using ManaxServer.Models.Serie;
using ManaxServer.Models.Tag;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxTests.Server.TagTests;

[TestClass]
public class SetSerieTagsTests : TagTestsSetup
{
    [TestMethod]
    public async Task SetSerieTagsWithValidDataSetsTagsForSerie()
    {
        Serie serie = Context.Series.First();
        Tag tag1 = new() { Name = "Action", ColorArgb = -1 };
        Tag tag2 = new() { Name = "Comedy", ColorArgb = -2 };
        Context.Tags.AddRange(tag1, tag2);
        await Context.SaveChangesAsync();

        TagOfSerieDto tagOfSerie = new()
        {
            SerieId = serie.Id,
            Tags = [
                new TagDto { Id = tag1.Id, Name = tag1.Name, ColorArgb = tag1.ColorArgb },
                new TagDto { Id = tag2.Id, Name = tag2.Name, ColorArgb = tag2.ColorArgb }
            ]
        };

        IActionResult result = await Controller.SetSerieTags(tagOfSerie);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        List<SerieTag> serieTags = await Context.SerieTags
            .Where(st => st.SerieId == serie.Id)
            .ToListAsync();

        Assert.AreEqual(2, serieTags.Count);
        Assert.IsTrue(serieTags.Any(st => st.TagId == tag1.Id));
        Assert.IsTrue(serieTags.Any(st => st.TagId == tag2.Id));
    }

    [TestMethod]
    public async Task SetSerieTagsWithNonExistentSerieReturnsNotFound()
    {
        Tag tag = new() { Name = "Action", ColorArgb = -1 };
        Context.Tags.Add(tag);
        await Context.SaveChangesAsync();

        TagOfSerieDto tagOfSerie = new()
        {
            SerieId = 999999,
            Tags = [new TagDto { Id = tag.Id, Name = tag.Name, ColorArgb = tag.ColorArgb }]
        };

        IActionResult result = await Controller.SetSerieTags(tagOfSerie);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task SetSerieTagsWithEmptyTagsListClearsAllTags()
    {
        Serie serie = Context.Series.First();
        Tag tag = new() { Name = "Action", ColorArgb = -1 };
        Context.Tags.Add(tag);
        await Context.SaveChangesAsync();

        SerieTag existingSerieTag = new()
        {
            SerieId = serie.Id,
            TagId = tag.Id
        };
        Context.SerieTags.Add(existingSerieTag);
        await Context.SaveChangesAsync();

        TagOfSerieDto tagOfSerie = new()
        {
            SerieId = serie.Id,
            Tags = []
        };

        IActionResult result = await Controller.SetSerieTags(tagOfSerie);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        List<SerieTag> serieTags = await Context.SerieTags
            .Where(st => st.SerieId == serie.Id)
            .ToListAsync();

        Assert.AreEqual(0, serieTags.Count);
    }

    [TestMethod]
    public async Task SetSerieTagsReplacesExistingTags()
    {
        Serie serie = Context.Series.First();
        Tag oldTag = new() { Name = "Old", ColorArgb = -1 };
        Tag newTag = new() { Name = "New", ColorArgb = -2 };
        Context.Tags.AddRange(oldTag, newTag);
        await Context.SaveChangesAsync();

        SerieTag existingSerieTag = new()
        {
            SerieId = serie.Id,
            TagId = oldTag.Id
        };
        Context.SerieTags.Add(existingSerieTag);
        await Context.SaveChangesAsync();

        TagOfSerieDto tagOfSerie = new()
        {
            SerieId = serie.Id,
            Tags = [new TagDto { Id = newTag.Id, Name = newTag.Name, ColorArgb = newTag.ColorArgb }]
        };

        IActionResult result = await Controller.SetSerieTags(tagOfSerie);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        List<SerieTag> serieTags = await Context.SerieTags
            .Where(st => st.SerieId == serie.Id)
            .ToListAsync();

        Assert.AreEqual(1, serieTags.Count);
        Assert.AreEqual(newTag.Id, serieTags.First().TagId);
        Assert.IsFalse(serieTags.Any(st => st.TagId == oldTag.Id));
    }

    [TestMethod]
    public async Task SetSerieTagsWithNonExistentTagSkipsTag()
    {
        Serie serie = Context.Series.First();
        Tag validTag = new() { Name = "Valid", ColorArgb = -1 };
        Context.Tags.Add(validTag);
        await Context.SaveChangesAsync();

        TagOfSerieDto tagOfSerie = new()
        {
            SerieId = serie.Id,
            Tags = [
                new TagDto { Id = validTag.Id, Name = validTag.Name, ColorArgb = validTag.ColorArgb },
                new TagDto { Id = 999999, Name = "NonExistent", ColorArgb = -2 }
            ]
        };

        IActionResult result = await Controller.SetSerieTags(tagOfSerie);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        List<SerieTag> serieTags = await Context.SerieTags
            .Where(st => st.SerieId == serie.Id)
            .ToListAsync();

        Assert.AreEqual(1, serieTags.Count);
        Assert.AreEqual(validTag.Id, serieTags.First().TagId);
    }

    [TestMethod]
    public async Task SetSerieTagsDoesNotAffectOtherSeriesTags()
    {
        Serie serie1 = Context.Series.First();
        Serie serie2 = Context.Series.Skip(1).First();
        Tag tag1 = new() { Name = "Tag1", ColorArgb = -1 };
        Tag tag2 = new() { Name = "Tag2", ColorArgb = -2 };
        Context.Tags.AddRange(tag1, tag2);
        await Context.SaveChangesAsync();

        SerieTag serie2Tag = new()
        {
            SerieId = serie2.Id,
            TagId = tag1.Id
        };
        Context.SerieTags.Add(serie2Tag);
        await Context.SaveChangesAsync();

        TagOfSerieDto tagOfSerie = new()
        {
            SerieId = serie1.Id,
            Tags = [new TagDto { Id = tag2.Id, Name = tag2.Name, ColorArgb = tag2.ColorArgb }]
        };

        IActionResult result = await Controller.SetSerieTags(tagOfSerie);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        List<SerieTag> serie1Tags = await Context.SerieTags
            .Where(st => st.SerieId == serie1.Id)
            .ToListAsync();
        Assert.AreEqual(1, serie1Tags.Count);
        Assert.AreEqual(tag2.Id, serie1Tags.First().TagId);

        List<SerieTag> serie2Tags = await Context.SerieTags
            .Where(st => st.SerieId == serie2.Id)
            .ToListAsync();
        Assert.AreEqual(1, serie2Tags.Count);
        Assert.AreEqual(tag1.Id, serie2Tags.First().TagId);
    }

    [TestMethod]
    public async Task SetSerieTagsWithDuplicateTagsInListReturnsInvalidOperation()
    {
        Serie serie = Context.Series.First();
        Tag tag = new() { Name = "Duplicate", ColorArgb = -1 };
        Context.Tags.Add(tag);
        await Context.SaveChangesAsync();

        TagOfSerieDto tagOfSerie = new()
        {
            SerieId = serie.Id,
            Tags = [
                new TagDto { Id = tag.Id, Name = tag.Name, ColorArgb = tag.ColorArgb },
                new TagDto { Id = tag.Id, Name = tag.Name, ColorArgb = tag.ColorArgb }
            ]
        };

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () =>
        {
            await Controller.SetSerieTags(tagOfSerie);
        });
    }

    [TestMethod]
    public async Task SetSerieTagsVerifySerieTagsAreCorrectlyLinked()
    {
        Serie serie = Context.Series.First();
        Tag tag = new() { Name = "Linked", ColorArgb = -1 };
        Context.Tags.Add(tag);
        await Context.SaveChangesAsync();

        TagOfSerieDto tagOfSerie = new()
        {
            SerieId = serie.Id,
            Tags = [new TagDto { Id = tag.Id, Name = tag.Name, ColorArgb = tag.ColorArgb }]
        };

        IActionResult result = await Controller.SetSerieTags(tagOfSerie);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        SerieTag? serieTag = await Context.SerieTags
            .FirstOrDefaultAsync(st => st.SerieId == serie.Id && st.TagId == tag.Id);

        Assert.IsNotNull(serieTag);
        Assert.AreEqual(serie.Id, serieTag.SerieId);
        Assert.AreEqual(tag.Id, serieTag.TagId);
    }

    [TestMethod]
    public async Task SetSerieTagsWithZeroSerieIdReturnsNotFound()
    {
        Tag tag = new() { Name = "Test", ColorArgb = -1 };
        Context.Tags.Add(tag);
        await Context.SaveChangesAsync();

        TagOfSerieDto tagOfSerie = new()
        {
            SerieId = 0,
            Tags = [new TagDto { Id = tag.Id, Name = tag.Name, ColorArgb = tag.ColorArgb }]
        };

        IActionResult result = await Controller.SetSerieTags(tagOfSerie);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }
}
