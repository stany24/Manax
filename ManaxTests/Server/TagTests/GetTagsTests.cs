using ManaxLibrary.DTO.Tag;
using ManaxServer.Models.Tag;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.Server.TagTests;

[TestClass]
public class GetTagsTests : TagTestsSetup
{
    [TestMethod]
    public async Task GetTagsReturnsAllTags()
    {
        Tag tag1 = new() { Name = "Action", ColorArgb = -16776961 };
        Tag tag2 = new() { Name = "Comedy", ColorArgb = -65536 };
        Context.Tags.AddRange(tag1, tag2);
        await Context.SaveChangesAsync();

        ActionResult<IEnumerable<TagDto>> result = await Controller.GetTags();
        IEnumerable<TagDto>? value = result.Value;
        Assert.IsNotNull(value);
        List<TagDto> tags = value.ToList();
        Assert.HasCount(2, tags);
        Assert.IsTrue(tags.Any(t => t.Name == "Action"));
        Assert.IsTrue(tags.Any(t => t.Name == "Comedy"));
    }

    [TestMethod]
    public async Task GetTagsReturnsEmptyListWhenNoTags()
    {
        ActionResult<IEnumerable<TagDto>> result = await Controller.GetTags();

        IEnumerable<TagDto>? value = result.Value;
        Assert.IsNotNull(value);
        List<TagDto> tags = value.ToList();
        Assert.IsEmpty(tags);
    }

    [TestMethod]
    public async Task GetTagsReturnsTagsWithCorrectProperties()
    {
        Tag tag = new() { Name = "Drama", ColorArgb = -16711936 };
        Context.Tags.Add(tag);
        await Context.SaveChangesAsync();

        ActionResult<IEnumerable<TagDto>> result = await Controller.GetTags();

        IEnumerable<TagDto>? value = result.Value;
        Assert.IsNotNull(value);
        List<TagDto> tags = value.ToList();
        TagDto returnedTag = tags.First();
        Assert.AreEqual("Drama", returnedTag.Name);
        Assert.AreEqual(-16711936, returnedTag.ColorArgb);
        Assert.IsGreaterThan(0, returnedTag.Id);
    }

    [TestMethod]
    public async Task GetTagsReturnsTagsInCorrectOrder()
    {
        Tag tag1 = new() { Name = "Z-Last", ColorArgb = -1 };
        Tag tag2 = new() { Name = "A-First", ColorArgb = -2 };
        Tag tag3 = new() { Name = "M-Middle", ColorArgb = -3 };
        Context.Tags.AddRange(tag1, tag2, tag3);
        await Context.SaveChangesAsync();

        ActionResult<IEnumerable<TagDto>> result = await Controller.GetTags();

        IEnumerable<TagDto>? value = result.Value;
        Assert.IsNotNull(value);
        List<TagDto> tags = value.ToList();
        Assert.HasCount(3, tags);
    }
}
