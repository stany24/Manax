using ManaxServer.Models.Rank;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.RankTests;

[TestClass]
public class DeleteRankTests: RankTestsSetup
{
    [TestMethod]
    public async Task DeleteRank_WithValidId_RemovesRank()
    {
        Rank rank = Context.Ranks.First();
        IActionResult result = await Controller.DeleteRank(rank.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Rank? deletedRank = await Context.Ranks.FindAsync(rank.Id);
        Assert.IsNull(deletedRank);
    }

    [TestMethod]
    public async Task DeleteRank_WithInvalidId_ReturnsNotFound()
    {
        IActionResult result = await Controller.DeleteRank(999999);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task DeleteRank_VerifyRankCountDecreases()
    {
        int initialCount = Context.Ranks.Count();
        Rank rank = Context.Ranks.First();

        IActionResult result = await Controller.DeleteRank(rank.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        int finalCount = Context.Ranks.Count();
        Assert.AreEqual(initialCount - 1, finalCount);
    }
    
    [TestMethod]
    public async Task DeleteRank_WithAssociatedUserRanks_RemovesUserRanks()
    {
        Rank rankWithUserRanks = Context.Ranks.First(r => Context.UserRanks.Any(ur => ur.RankId == r.Id));

        IActionResult result = await Controller.DeleteRank(rankWithUserRanks.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Rank? deletedRank = await Context.Ranks.FindAsync(rankWithUserRanks.Id);
        Assert.IsNull(deletedRank);

        int finalUserRankCount = Context.UserRanks.Count(ur => ur.RankId == rankWithUserRanks.Id);
        Assert.AreEqual(0, finalUserRankCount);
    }
}