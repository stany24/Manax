using ManaxLibrary.DTO.Rank;
using ManaxServer.Models.Rank;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.RankTests;

[TestClass]
public class UpdateRankTests : RankTestsSetup
{
    [TestMethod]
    public async Task UpdateRankWithValidIdUpdatesRank()
    {
        Rank existingRank = Context.Ranks.First();
        RankUpdateDto updateRank = new()
        {
            Id = existingRank.Id,
            Value = 8,
            Name = "Updated Rank"
        };

        IActionResult result = await Controller.UpdateRank(updateRank);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Rank? updatedRank = await Context.Ranks.FindAsync(existingRank.Id);
        Assert.IsNotNull(updatedRank);
        Assert.AreEqual(updateRank.Value, updatedRank.Value);
        Assert.AreEqual(updateRank.Name, updatedRank.Name);
    }

    [TestMethod]
    public async Task UpdateRankWithInvalidIdReturnsNotFound()
    {
        RankUpdateDto updateRank = new()
        {
            Id = 999999,
            Value = 8,
            Name = "Updated Rank"
        };

        IActionResult result = await Controller.UpdateRank(updateRank);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task UpdateRankWithExistingValueFromAnotherRankReturnsBadRequest()
    {
        List<Rank> ranks = Context.Ranks.Take(2).ToList();
        Rank firstRank = ranks[0];
        Rank secondRank = ranks[1];

        RankUpdateDto updateRank = new()
        {
            Id = firstRank.Id,
            Value = secondRank.Value,
            Name = "Updated Name"
        };

        IActionResult result = await Controller.UpdateRank(updateRank);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task UpdateRankWithExistingNameFromAnotherRankReturnsBadRequest()
    {
        List<Rank> ranks = Context.Ranks.Take(2).ToList();
        Rank firstRank = ranks[0];
        Rank secondRank = ranks[1];

        RankUpdateDto updateRank = new()
        {
            Id = firstRank.Id,
            Value = 99,
            Name = secondRank.Name
        };

        IActionResult result = await Controller.UpdateRank(updateRank);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }
}