using ManaxServer.Models.Rank;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.RankTests;

[TestClass]
public class UpdateRankTests: RankTestsSetup
{
    [TestMethod]
    public async Task UpdateRank_WithValidId_UpdatesRank()
    {
        Rank existingRank = Context.Ranks.First();
        Rank updateRank = new()
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
    public async Task UpdateRank_WithInvalidId_ReturnsNotFound()
    {
        Rank updateRank = new()
        {
            Id = 999999,
            Value = 8,
            Name = "Updated Rank"
        };

        IActionResult result = await Controller.UpdateRank(updateRank);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }
    
    [TestMethod]
    public async Task UpdateRank_WithExistingValueFromAnotherRank_ReturnsBadRequest()
    {
        List<Rank> ranks = Context.Ranks.Take(2).ToList();
        Rank firstRank = ranks[0];
        Rank secondRank = ranks[1];

        Rank updateRank = new()
        {
            Id = firstRank.Id,
            Value = secondRank.Value,
            Name = "Updated Name"
        };

        IActionResult result = await Controller.UpdateRank(updateRank);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task UpdateRank_WithExistingNameFromAnotherRank_ReturnsBadRequest()
    {
        List<Rank> ranks = Context.Ranks.Take(2).ToList();
        Rank firstRank = ranks[0];
        Rank secondRank = ranks[1];

        Rank updateRank = new()
        {
            Id = firstRank.Id,
            Value = 99,
            Name = secondRank.Name
        };

        IActionResult result = await Controller.UpdateRank(updateRank);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }
}