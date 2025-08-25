using ManaxLibrary.DTO.Rank;
using ManaxServer.Controllers;
using ManaxServer.Models;
using ManaxServer.Models.Rank;
using ManaxServer.Services.Mapper;
using ManaxServer.Services.Notification;
using ManaxTests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace ManaxTests;

[TestClass]
public class TestRankController
{
    private ManaxContext _context = null!;
    private RankController _controller = null!;
    private ManaxMapper _mapper = null!;
    private Mock<INotificationService> _mockNotificationService = null!;

    [TestInitialize]
    public void Setup()
    {
        _context = SqliteTestDbContextFactory.CreateTestContext();

        _mapper = new ManaxMapper(new ManaxMapping());
        _mockNotificationService = new Mock<INotificationService>();

        _controller = new RankController(_context, _mapper, _mockNotificationService.Object);
        
        ClaimsPrincipal user = new(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "TestUser1")
        ], "test"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = user
            }
        };
    }

    [TestCleanup]
    public void Cleanup()
    {
        SqliteTestDbContextFactory.CleanupTestDatabase(_context);
    }

    [TestMethod]
    public async Task GetRanks_ReturnsAllRanks()
    {
        ActionResult<IEnumerable<RankDto>> result = await _controller.GetRanks();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        List<RankDto>? returnedRanks = result.Value as List<RankDto>;
        Assert.IsNotNull(returnedRanks);
        Assert.AreEqual(_context.Ranks.Count(), returnedRanks.Count);
        foreach (Rank rank in _context.Ranks)
        {
            RankDto? returnedRank = returnedRanks.FirstOrDefault(r => r.Id == rank.Id);
            Assert.IsNotNull(returnedRank);
            Assert.AreEqual(rank.Value, returnedRank.Value);
            Assert.AreEqual(rank.Name, returnedRank.Name);
        }
    }

    [TestMethod]
    public async Task CreateRank_WithValidData_CreatesRank()
    {
        RankCreateDto createDto = new()
        {
            Value = 7,
            Name = "Good"
        };

        ActionResult<long> result = await _controller.CreateRank(createDto);

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        long? rankId = result.Value;
        Assert.IsNotNull(rankId);

        Rank? createdRank = await _context.Ranks.FindAsync(rankId);
        Assert.IsNotNull(createdRank);
        Assert.AreEqual(createDto.Value, createdRank.Value);
        Assert.AreEqual(createDto.Name, createdRank.Name);
    }

    [TestMethod]
    public async Task CreateRank_WithDuplicateValue_ThrowsException()
    {
        Rank existingRank = _context.Ranks.First();
        RankCreateDto createDto = new()
        {
            Value = existingRank.Value,
            Name = "Duplicate Value"
        };
        
        await Assert.ThrowsExactlyAsync<DbUpdateException>(() => _controller.CreateRank(createDto));
    }

    [TestMethod]
    public async Task CreateRank_WithDuplicateName_ThrowsException()
    {
        Rank existingRank = _context.Ranks.First();
        RankCreateDto createDto = new()
        {
            Value = 99,
            Name = existingRank.Name
        };
        
        await Assert.ThrowsExactlyAsync<DbUpdateException>(() => _controller.CreateRank(createDto));
    }

    [TestMethod]
    public async Task UpdateRank_WithValidId_UpdatesRank()
    {
        Rank existingRank = _context.Ranks.First();
        Rank updateRank = new()
        {
            Id = existingRank.Id,
            Value = 8,
            Name = "Updated Rank"
        };

        IActionResult result = await _controller.UpdateRank(existingRank.Id, updateRank);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Rank? updatedRank = await _context.Ranks.FindAsync(existingRank.Id);
        Assert.IsNotNull(updatedRank);
        Assert.AreEqual(updateRank.Value, updatedRank.Value);
        Assert.AreEqual(updateRank.Name, updatedRank.Name);
    }

    [TestMethod]
    public async Task UpdateRank_WithMismatchedId_ReturnsBadRequest()
    {
        Rank existingRank = _context.Ranks.First();
        Rank updateRank = new()
        {
            Id = existingRank.Id + 1,
            Value = 8,
            Name = "Updated Rank"
        };

        IActionResult result = await _controller.UpdateRank(existingRank.Id, updateRank);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
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

        IActionResult result = await _controller.UpdateRank(999999, updateRank);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task DeleteRank_WithValidId_RemovesRank()
    {
        Rank rank = _context.Ranks.First();
        IActionResult result = await _controller.DeleteRank(rank.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Rank? deletedRank = await _context.Ranks.FindAsync(rank.Id);
        Assert.IsNull(deletedRank);
    }

    [TestMethod]
    public async Task DeleteRank_WithInvalidId_ReturnsNotFound()
    {
        IActionResult result = await _controller.DeleteRank(999999);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task DeleteRank_VerifyRankCountDecreases()
    {
        int initialCount = _context.Ranks.Count();
        Rank rank = _context.Ranks.First();

        IActionResult result = await _controller.DeleteRank(rank.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        int finalCount = _context.Ranks.Count();
        Assert.AreEqual(initialCount - 1, finalCount);
    }

    [TestMethod]
    public async Task SetUserRank_WithNewRank_CreatesUserRank()
    {
        UserRankCreateDto createDto = new()
        {
            SerieId = 2,
            RankId = 1
        };

        IActionResult result = await _controller.SetUserRank(createDto);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        UserRank? userRank = _context.UserRanks.FirstOrDefault(ur => ur.SerieId == createDto.SerieId && ur.RankId == createDto.RankId);
        Assert.IsNotNull(userRank);
        Assert.AreEqual(createDto.SerieId, userRank.SerieId);
        Assert.AreEqual(createDto.RankId, userRank.RankId);
    }

    [TestMethod]
    public async Task SetUserRank_WithExistingRank_UpdatesUserRank()
    {
        UserRankCreateDto updateDto = new()
        {
            SerieId = 1,
            RankId = 3
        };

        IActionResult result = await _controller.SetUserRank(updateDto);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        UserRank? userRank = _context.UserRanks.FirstOrDefault(ur => ur.SerieId == updateDto.SerieId);
        Assert.IsNotNull(userRank);
        Assert.AreEqual(updateDto.RankId, userRank.RankId);
    }

    [TestMethod]
    public async Task GetRanking_ReturnsUserRanks()
    {
        ActionResult<IEnumerable<UserRankDto>> result = await _controller.GetRanking();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        List<UserRankDto>? returnedRanks = result.Value as List<UserRankDto>;
        Assert.IsNotNull(returnedRanks);

        List<UserRank> expectedUserRanks = _context.UserRanks.Where(ur => ur.UserId == 1).ToList();
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
    public async Task CreateRank_VerifyRankCountIncreases()
    {
        int initialCount = _context.Ranks.Count();
        RankCreateDto createDto = new()
        {
            Value = 15,
            Name = "Masterpiece"
        };

        ActionResult<long> result = await _controller.CreateRank(createDto);

        long? rankId = result.Value;
        Assert.IsNotNull(rankId);

        int finalCount = _context.Ranks.Count();
        Assert.AreEqual(initialCount + 1, finalCount);
    }

    [TestMethod]
    public async Task UpdateRank_WithExistingValueFromAnotherRank_ReturnsBadRequest()
    {
        List<Rank> ranks = _context.Ranks.Take(2).ToList();
        Rank firstRank = ranks[0];
        Rank secondRank = ranks[1];

        Rank updateRank = new()
        {
            Id = firstRank.Id,
            Value = secondRank.Value,
            Name = "Updated Name"
        };

        IActionResult result = await _controller.UpdateRank(firstRank.Id, updateRank);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task UpdateRank_WithExistingNameFromAnotherRank_ReturnsBadRequest()
    {
        List<Rank> ranks = _context.Ranks.Take(2).ToList();
        Rank firstRank = ranks[0];
        Rank secondRank = ranks[1];

        Rank updateRank = new()
        {
            Id = firstRank.Id,
            Value = 99,
            Name = secondRank.Name
        };

        IActionResult result = await _controller.UpdateRank(firstRank.Id, updateRank);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task GetRanks_VerifyCorrectRankMapping()
    {
        ActionResult<IEnumerable<RankDto>> result = await _controller.GetRanks();

        List<RankDto>? returnedRanks = result.Value as List<RankDto>;
        Assert.IsNotNull(returnedRanks);

        Rank expectedRank = _context.Ranks.First();
        RankDto? returnedRank = returnedRanks.FirstOrDefault(r => r.Id == expectedRank.Id);
        Assert.IsNotNull(returnedRank);
        Assert.AreEqual(expectedRank.Id, returnedRank.Id);
        Assert.AreEqual(expectedRank.Value, returnedRank.Value);
        Assert.AreEqual(expectedRank.Name, returnedRank.Name);
    }

    [TestMethod]
    public async Task SetUserRank_VerifyUserRankCountChanges()
    {
        int initialCount = _context.UserRanks.Count();
        UserRankCreateDto createDto = new()
        {
            SerieId = 3,
            RankId = 2
        };

        IActionResult result = await _controller.SetUserRank(createDto);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        int finalCount = _context.UserRanks.Count();
        Assert.AreEqual(initialCount + 1, finalCount);
    }

    [TestMethod]
    public async Task DeleteRank_WithAssociatedUserRanks_RemovesUserRanks()
    {
        Rank rankWithUserRanks = _context.Ranks.First(r => _context.UserRanks.Any(ur => ur.RankId == r.Id));

        IActionResult result = await _controller.DeleteRank(rankWithUserRanks.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Rank? deletedRank = await _context.Ranks.FindAsync(rankWithUserRanks.Id);
        Assert.IsNull(deletedRank);

        int finalUserRankCount = _context.UserRanks.Count(ur => ur.RankId == rankWithUserRanks.Id);
        Assert.AreEqual(0, finalUserRankCount);
    }
}