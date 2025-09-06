using ManaxLibrary.DTO.Rank;
using ManaxServer.Models.Rank;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.RankTests;

[TestClass]
public class GetRankTests: RankTestsSetup
{
    [TestMethod]
    public async Task GetRanking_ReturnsUserRanks()
    {
        ActionResult<IEnumerable<UserRankDto>> result = await Controller.GetRanking();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        List<UserRankDto>? returnedRanks = result.Value as List<UserRankDto>;
        Assert.IsNotNull(returnedRanks);

        List<UserRank> expectedUserRanks = Context.UserRanks.Where(ur => ur.UserId == 1).ToList();
        Assert.AreEqual(expectedUserRanks.Count, returnedRanks.Count);

        foreach (UserRank expectedRank in expectedUserRanks)
        {
            UserRankDto? returnedRank = returnedRanks.FirstOrDefault(r =>
                r.SerieId == expectedRank.SerieId && r.RankId == expectedRank.RankId);
            Assert.IsNotNull(returnedRank);
            Assert.AreEqual(expectedRank.UserId, returnedRank.UserId);
        }
    }
    
    [TestMethod]
    public async Task GetRanks_ReturnsAllRanks()
    {
        ActionResult<IEnumerable<RankDto>> result = await Controller.GetRanks();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        List<RankDto>? returnedRanks = result.Value as List<RankDto>;
        Assert.IsNotNull(returnedRanks);
        Assert.AreEqual(Context.Ranks.Count(), returnedRanks.Count);
        foreach (Rank rank in Context.Ranks)
        {
            RankDto? returnedRank = returnedRanks.FirstOrDefault(r => r.Id == rank.Id);
            Assert.IsNotNull(returnedRank);
            Assert.AreEqual(rank.Value, returnedRank.Value);
            Assert.AreEqual(rank.Name, returnedRank.Name);
        }
    }

    [TestMethod]
    public async Task GetRanks_VerifyCorrectRankMapping()
    {
        ActionResult<IEnumerable<RankDto>> result = await Controller.GetRanks();

        List<RankDto>? returnedRanks = result.Value as List<RankDto>;
        Assert.IsNotNull(returnedRanks);

        Rank expectedRank = Context.Ranks.First();
        RankDto? returnedRank = returnedRanks.FirstOrDefault(r => r.Id == expectedRank.Id);
        Assert.IsNotNull(returnedRank);
        Assert.AreEqual(expectedRank.Id, returnedRank.Id);
        Assert.AreEqual(expectedRank.Value, returnedRank.Value);
        Assert.AreEqual(expectedRank.Name, returnedRank.Name);
    }
}