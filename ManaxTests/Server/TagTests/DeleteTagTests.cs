using ManaxServer.Models.Tag;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.Server.TagTests;

[TestClass]
public class DeleteTagTests : TagTestsSetup
{
    [TestMethod]
    public async Task DeleteTagWithValidIdDeletesTag()
    {
        Tag tag = new() { Name = "ToDelete", ColorArgb = -1 };
        Context.Tags.Add(tag);
        await Context.SaveChangesAsync();

        IActionResult result = await Controller.DeleteTag(tag.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Tag? deletedTag = await Context.Tags.FindAsync(tag.Id);
        Assert.IsNull(deletedTag);
    }

    [TestMethod]
    public async Task DeleteTagWithNonExistentIdReturnsNotFound()
    {
        IActionResult result = await Controller.DeleteTag(999999);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task DeleteTagVerifyNotificationIsSent()
    {
        Tag tag = new() { Name = "NotifyDelete", ColorArgb = -1 };
        Context.Tags.Add(tag);
        await Context.SaveChangesAsync();

        long tagId = tag.Id;

        IActionResult result = await Controller.DeleteTag(tagId);

        Assert.IsInstanceOfType(result, typeof(OkResult));
        Assert.AreEqual(tagId, MockNotificationService.TagDeletedId);
    }

    [TestMethod]
    public async Task DeleteTagVerifyDatabaseCountDecreases()
    {
        Tag tag = new() { Name = "CountTest", ColorArgb = -1 };
        Context.Tags.Add(tag);
        await Context.SaveChangesAsync();

        int initialCount = Context.Tags.Count();

        IActionResult result = await Controller.DeleteTag(tag.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        int finalCount = Context.Tags.Count();
        Assert.AreEqual(initialCount - 1, finalCount);
    }

    [TestMethod]
    public async Task DeleteTagWithZeroIdReturnsNotFound()
    {
        IActionResult result = await Controller.DeleteTag(0);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task DeleteTagWithNegativeIdReturnsNotFound()
    {
        IActionResult result = await Controller.DeleteTag(-1);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task DeleteTagDoesNotAffectOtherTags()
    {
        Tag tag1 = new() { Name = "Keep1", ColorArgb = -1 };
        Tag tag2 = new() { Name = "Delete", ColorArgb = -2 };
        Tag tag3 = new() { Name = "Keep2", ColorArgb = -3 };
        Context.Tags.AddRange(tag1, tag2, tag3);
        await Context.SaveChangesAsync();

        IActionResult result = await Controller.DeleteTag(tag2.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Tag? deletedTag = await Context.Tags.FindAsync(tag2.Id);
        Assert.IsNull(deletedTag);

        Tag? keepTag1 = await Context.Tags.FindAsync(tag1.Id);
        Assert.IsNotNull(keepTag1);

        Tag? keepTag2 = await Context.Tags.FindAsync(tag3.Id);
        Assert.IsNotNull(keepTag2);
    }

    [TestMethod]
    public async Task DeleteTagMultipleTimesWithSameIdReturnsNotFoundOnSecond()
    {
        Tag tag = new() { Name = "DeleteTwice", ColorArgb = -1 };
        Context.Tags.Add(tag);
        await Context.SaveChangesAsync();

        long tagId = tag.Id;

        IActionResult result1 = await Controller.DeleteTag(tagId);
        Assert.IsInstanceOfType(result1, typeof(OkResult));

        IActionResult result2 = await Controller.DeleteTag(tagId);
        Assert.IsInstanceOfType(result2, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task DeleteTagVerifyCorrectTagIsDeleted()
    {
        Tag tag1 = new() { Name = "Tag1", ColorArgb = -1 };
        Tag tag2 = new() { Name = "Tag2", ColorArgb = -2 };
        Context.Tags.AddRange(tag1, tag2);
        await Context.SaveChangesAsync();

        IActionResult result = await Controller.DeleteTag(tag1.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Tag? deletedTag = await Context.Tags.FindAsync(tag1.Id);
        Assert.IsNull(deletedTag);

        Tag? remainingTag = await Context.Tags.FindAsync(tag2.Id);
        Assert.IsNotNull(remainingTag);
        Assert.AreEqual("Tag2", remainingTag.Name);
    }
}