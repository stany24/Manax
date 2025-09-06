using ManaxLibrary.DTO.Rank;
using ManaxServer.Models.Rank;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxTests.RankTests;

[TestClass]
public class CreateRankTests: RankTestsSetup
{
    [TestMethod]
    public async Task CreateRank_WithValidData_CreatesRank()
    {
        RankCreateDto createDto = new()
        {
            Value = 7,
            Name = "Good"
        };

        ActionResult<long> result = await Controller.CreateRank(createDto);

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        long? rankId = result.Value;
        Assert.IsNotNull(rankId);

        Rank? createdRank = await Context.Ranks.FindAsync(rankId);
        Assert.IsNotNull(createdRank);
        Assert.AreEqual(createDto.Value, createdRank.Value);
        Assert.AreEqual(createDto.Name, createdRank.Name);
    }

    [TestMethod]
    public async Task CreateRank_WithDuplicateValue_ThrowsException()
    {
        Rank existingRank = Context.Ranks.First();
        RankCreateDto createDto = new()
        {
            Value = existingRank.Value,
            Name = "Duplicate Value"
        };

        await Assert.ThrowsExactlyAsync<DbUpdateException>(() => Controller.CreateRank(createDto));
    }

    [TestMethod]
    public async Task CreateRank_WithDuplicateName_ThrowsException()
    {
        Rank existingRank = Context.Ranks.First();
        RankCreateDto createDto = new()
        {
            Value = 99,
            Name = existingRank.Name
        };

        await Assert.ThrowsExactlyAsync<DbUpdateException>(() => Controller.CreateRank(createDto));
    }
    
    [TestMethod]
    public async Task CreateRank_VerifyRankCountIncreases()
    {
        int initialCount = Context.Ranks.Count();
        RankCreateDto createDto = new()
        {
            Value = 15,
            Name = "Masterpiece"
        };

        ActionResult<long> result = await Controller.CreateRank(createDto);

        long? rankId = result.Value;
        Assert.IsNotNull(rankId);

        int finalCount = Context.Ranks.Count();
        Assert.AreEqual(initialCount + 1, finalCount);
    }
}