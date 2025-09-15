using ManaxLibrary.DTO.Rank;
using ManaxServer.Models.Rank;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.RankTests;

[TestClass]
public class SetRankTests : RankTestsSetup
{
    [TestMethod]
    public async Task SetUserRankWithNewRankCreatesUserRank()
    {
        UserRankCreateDto createDto = new()
        {
            SerieId = 2,
            RankId = 1
        };

        IActionResult result = await Controller.SetUserRank(createDto);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        UserRank? userRank =
            Context.UserRanks.FirstOrDefault(ur => ur.SerieId == createDto.SerieId && ur.RankId == createDto.RankId);
        Assert.IsNotNull(userRank);
        Assert.AreEqual(createDto.SerieId, userRank.SerieId);
        Assert.AreEqual(createDto.RankId, userRank.RankId);
    }

    [TestMethod]
    public async Task SetUserRankWithExistingRankUpdatesUserRank()
    {
        UserRankCreateDto updateDto = new()
        {
            SerieId = 1,
            RankId = 3
        };

        IActionResult result = await Controller.SetUserRank(updateDto);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        UserRank? userRank = Context.UserRanks.FirstOrDefault(ur => ur.SerieId == updateDto.SerieId);
        Assert.IsNotNull(userRank);
        Assert.AreEqual(updateDto.RankId, userRank.RankId);
    }

    [TestMethod]
    public async Task SetUserRankVerifyUserRankCountChanges()
    {
        int initialCount = Context.UserRanks.Count();
        UserRankCreateDto createDto = new()
        {
            SerieId = 3,
            RankId = 2
        };

        IActionResult result = await Controller.SetUserRank(createDto);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        int finalCount = Context.UserRanks.Count();
        Assert.AreEqual(initialCount + 1, finalCount);
    }
}