using ManaxLibrary.DTO.Tag;
using ManaxServer.Models.Tag;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxTests.Server.TagTests;

[TestClass]
public class CreateTagTests : TagTestsSetup
{
    [TestMethod]
    public async Task CreateTagWithValidDataCreatesNewTag()
    {
        TagCreateDto tagCreate = new()
        {
            Name = "Adventure",
            ColorArgb = -16776961
        };

        IActionResult result = await Controller.CreateTag(tagCreate);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Tag? createdTag = await Context.Tags.FirstOrDefaultAsync(t => t.Name == "Adventure");
        Assert.IsNotNull(createdTag);
        Assert.AreEqual("Adventure", createdTag.Name);
        Assert.AreEqual(-16776961, createdTag.ColorArgb);
    }

    [TestMethod]
    public async Task CreateTagVerifyNotificationIsSent()
    {
        TagCreateDto tagCreate = new()
        {
            Name = "Horror",
            ColorArgb = -65536
        };

        IActionResult result = await Controller.CreateTag(tagCreate);

        Assert.IsInstanceOfType(result, typeof(OkResult));
        Assert.IsNotNull(MockNotificationService.TagCreated);
        Assert.AreEqual("Horror", MockNotificationService.TagCreated.Name);
        Assert.AreEqual(-65536, MockNotificationService.TagCreated.ColorArgb);
    }

    [TestMethod]
    public async Task CreateTagWithDuplicateNameThrowsException()
    {
        Tag existingTag = new() { Name = "Action", ColorArgb = -1 };
        Context.Tags.Add(existingTag);
        await Context.SaveChangesAsync();

        TagCreateDto tagCreate = new()
        {
            Name = "Action",
            ColorArgb = -16776961
        };

        await Assert.ThrowsExactlyAsync<DbUpdateException>(async () =>
        {
            await Controller.CreateTag(tagCreate);
        });
    }

    [TestMethod]
    public async Task CreateTagWithZeroColorCreatesTag()
    {
        TagCreateDto tagCreate = new()
        {
            Name = "Neutral",
            ColorArgb = 0
        };

        IActionResult result = await Controller.CreateTag(tagCreate);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Tag? createdTag = await Context.Tags.FirstOrDefaultAsync(t => t.Name == "Neutral");
        Assert.IsNotNull(createdTag);
        Assert.AreEqual(0, createdTag.ColorArgb);
    }

    [TestMethod]
    public async Task CreateTagWithNegativeColorCreatesTag()
    {
        TagCreateDto tagCreate = new()
        {
            Name = "Dark",
            ColorArgb = -1
        };

        IActionResult result = await Controller.CreateTag(tagCreate);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Tag? createdTag = await Context.Tags.FirstOrDefaultAsync(t => t.Name == "Dark");
        Assert.IsNotNull(createdTag);
        Assert.AreEqual(-1, createdTag.ColorArgb);
    }

    [TestMethod]
    public async Task CreateTagVerifyDatabaseCountIncreases()
    {
        int initialCount = Context.Tags.Count();

        TagCreateDto tagCreate = new()
        {
            Name = "Fantasy",
            ColorArgb = -16711681
        };

        IActionResult result = await Controller.CreateTag(tagCreate);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        int finalCount = Context.Tags.Count();
        Assert.AreEqual(initialCount + 1, finalCount);
    }

    [TestMethod]
    public async Task CreateTagWithLongNameCreatesTag()
    {
        TagCreateDto tagCreate = new()
        {
            Name = "Very Long Tag Name That Should Still Be Valid",
            ColorArgb = -16776961
        };

        IActionResult result = await Controller.CreateTag(tagCreate);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Tag? createdTag = await Context.Tags.FirstOrDefaultAsync(t => t.Name == "Very Long Tag Name That Should Still Be Valid");
        Assert.IsNotNull(createdTag);
    }

    [TestMethod]
    public async Task CreateTagVerifyCorrectIdIsAssigned()
    {
        TagCreateDto tagCreate = new()
        {
            Name = "TestId",
            ColorArgb = -1
        };

        IActionResult result = await Controller.CreateTag(tagCreate);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Tag? createdTag = await Context.Tags.FirstOrDefaultAsync(t => t.Name == "TestId");
        Assert.IsNotNull(createdTag);
        Assert.IsTrue(createdTag.Id > 0);
    }
}
