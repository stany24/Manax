using ManaxLibrary.DTO.Rank;
using ManaxServer.Models.Rank;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.Server.RankTests;

[TestClass]
public class GetRankTests : RankTestsSetup
{
    [TestMethod]
    public async Task GetRankingReturnsUserRanks()
    {
        ActionResult<IEnumerable<UserRankDto>> result = await Controller.GetRanking();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);

        List<UserRankDto>? returnedRanks = okResult.Value as List<UserRankDto>;
        Assert.IsNotNull(returnedRanks);

        List<UserRank> expectedUserRanks = Context.UserRanks.Where(ur => ur.UserId == 1).ToList();
        Assert.HasCount(expectedUserRanks.Count, returnedRanks);

        foreach (UserRank expectedRank in expectedUserRanks)
        {
            UserRankDto? returnedRank = returnedRanks.FirstOrDefault(r =>
                r.SerieId == expectedRank.SerieId && r.RankId == expectedRank.RankId);
            Assert.IsNotNull(returnedRank);
            Assert.AreEqual(expectedRank.UserId, returnedRank.UserId);
        }
    }

    [TestMethod]
    public async Task GetRanksReturnsAllRanks()
    {
        ActionResult<IEnumerable<RankDto>> result = await Controller.GetRanks();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);

        List<RankDto>? returnedRanks = okResult.Value as List<RankDto>;
        Assert.IsNotNull(returnedRanks);
        Assert.HasCount(Context.Ranks.Count(), returnedRanks);
        foreach (Rank rank in Context.Ranks)
        {
            RankDto? returnedRank = returnedRanks.FirstOrDefault(r => r.Id == rank.Id);
            Assert.IsNotNull(returnedRank);
            Assert.AreEqual(rank.Value, returnedRank.Value);
            Assert.AreEqual(rank.Name, returnedRank.Name);
        }
    }

    [TestMethod]
    public async Task GetRanksVerifyCorrectRankMapping()
    {
        ActionResult<IEnumerable<RankDto>> result = await Controller.GetRanks();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        
        List<RankDto>? returnedRanks = okResult.Value as List<RankDto>;
        Assert.IsNotNull(returnedRanks);

        Rank expectedRank = Context.Ranks.First();
        RankDto? returnedRank = returnedRanks.FirstOrDefault(r => r.Id == expectedRank.Id);
        Assert.IsNotNull(returnedRank);
        Assert.AreEqual(expectedRank.Id, returnedRank.Id);
        Assert.AreEqual(expectedRank.Value, returnedRank.Value);
        Assert.AreEqual(expectedRank.Name, returnedRank.Name);
    }
}