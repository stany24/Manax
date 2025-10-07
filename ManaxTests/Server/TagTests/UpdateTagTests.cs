using ManaxLibrary.DTO.Tag;
using ManaxServer.Models.Tag;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.Server.TagTests;

[TestClass]
public class UpdateTagTests : TagTestsSetup
{
    [TestMethod]
    public async Task UpdateTagWithValidDataUpdatesExistingTag()
    {
        Tag tag = new() { Name = "Action", ColorArgb = -16776961 };
        Context.Tags.Add(tag);
        await Context.SaveChangesAsync();

        TagUpdateDto tagUpdate = new()
        {
            Id = tag.Id,
            Name = "Updated Action",
            ColorArgb = -65536
        };

        IActionResult result = await Controller.UpdateTag(tagUpdate);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Tag? updatedTag = await Context.Tags.FindAsync(tag.Id);
        Assert.IsNotNull(updatedTag);
        Assert.AreEqual("Updated Action", updatedTag.Name);
        Assert.AreEqual(-65536, updatedTag.ColorArgb);
    }

    [TestMethod]
    public async Task UpdateTagWithNonExistentIdReturnsNotFound()
    {
        TagUpdateDto tagUpdate = new()
        {
            Id = 999999,
            Name = "Non Existent",
            ColorArgb = -1
        };

        IActionResult result = await Controller.UpdateTag(tagUpdate);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task UpdateTagVerifyNotificationIsSent()
    {
        Tag tag = new() { Name = "Comedy", ColorArgb = -16711936 };
        Context.Tags.Add(tag);
        await Context.SaveChangesAsync();

        TagUpdateDto tagUpdate = new()
        {
            Id = tag.Id,
            Name = "Updated Comedy",
            ColorArgb = -16711681
        };

        IActionResult result = await Controller.UpdateTag(tagUpdate);

        Assert.IsInstanceOfType(result, typeof(OkResult));
        Assert.IsNotNull(MockNotificationService.TagUpdated);
        Assert.AreEqual("Updated Comedy", MockNotificationService.TagUpdated.Name);
        Assert.AreEqual(-16711681, MockNotificationService.TagUpdated.ColorArgb);
    }

    [TestMethod]
    public async Task UpdateTagWithDuplicateNameReturnsBadRequest()
    {
        Tag tag1 = new() { Name = "Action", ColorArgb = -1 };
        Tag tag2 = new() { Name = "Comedy", ColorArgb = -2 };
        Context.Tags.AddRange(tag1, tag2);
        await Context.SaveChangesAsync();

        TagUpdateDto tagUpdate = new()
        {
            Id = tag2.Id,
            Name = "Action",
            ColorArgb = -3
        };

        IActionResult result = await Controller.UpdateTag(tagUpdate);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task UpdateTagOnlyNameUpdatesCorrectly()
    {
        Tag tag = new() { Name = "Drama", ColorArgb = -16711936 };
        Context.Tags.Add(tag);
        await Context.SaveChangesAsync();

        TagUpdateDto tagUpdate = new()
        {
            Id = tag.Id,
            Name = "Updated Drama",
            ColorArgb = tag.ColorArgb
        };

        IActionResult result = await Controller.UpdateTag(tagUpdate);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Tag? updatedTag = await Context.Tags.FindAsync(tag.Id);
        Assert.IsNotNull(updatedTag);
        Assert.AreEqual("Updated Drama", updatedTag.Name);
        Assert.AreEqual(-16711936, updatedTag.ColorArgb);
    }

    [TestMethod]
    public async Task UpdateTagOnlyColorUpdatesCorrectly()
    {
        Tag tag = new() { Name = "Horror", ColorArgb = -16776961 };
        Context.Tags.Add(tag);
        await Context.SaveChangesAsync();

        TagUpdateDto tagUpdate = new()
        {
            Id = tag.Id,
            Name = tag.Name,
            ColorArgb = -65536
        };

        IActionResult result = await Controller.UpdateTag(tagUpdate);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Tag? updatedTag = await Context.Tags.FindAsync(tag.Id);
        Assert.IsNotNull(updatedTag);
        Assert.AreEqual("Horror", updatedTag.Name);
        Assert.AreEqual(-65536, updatedTag.ColorArgb);
    }

    [TestMethod]
    public async Task UpdateTagVerifyDatabaseCountRemainsSame()
    {
        Tag tag = new() { Name = "Sci-Fi", ColorArgb = -1 };
        Context.Tags.Add(tag);
        await Context.SaveChangesAsync();

        int initialCount = Context.Tags.Count();

        TagUpdateDto tagUpdate = new()
        {
            Id = tag.Id,
            Name = "Updated Sci-Fi",
            ColorArgb = -2
        };

        IActionResult result = await Controller.UpdateTag(tagUpdate);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        int finalCount = Context.Tags.Count();
        Assert.AreEqual(initialCount, finalCount);
    }

    [TestMethod]
    public async Task UpdateTagWithZeroIdReturnsNotFound()
    {
        TagUpdateDto tagUpdate = new()
        {
            Id = 0,
            Name = "Zero Id",
            ColorArgb = -1
        };

        IActionResult result = await Controller.UpdateTag(tagUpdate);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task UpdateTagWithNegativeIdReturnsNotFound()
    {
        TagUpdateDto tagUpdate = new()
        {
            Id = -1,
            Name = "Negative Id",
            ColorArgb = -1
        };

        IActionResult result = await Controller.UpdateTag(tagUpdate);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }
}