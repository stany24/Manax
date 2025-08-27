using System.Security.Claims;
using ManaxLibrary.DTO.Issue.Automatic;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxServer.Controllers;
using ManaxServer.Models;
using ManaxServer.Models.Issue.Automatic;
using ManaxServer.Models.Issue.Reported;
using ManaxServer.Services.Mapper;
using ManaxServer.Services.Notification;
using ManaxTests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ManaxTests;

[TestClass]
public class TestIssueController
{
    private ManaxContext _context = null!;
    private IssueController _controller = null!;
    private ManaxMapper _mapper = null!;
    private Mock<INotificationService> _mockNotificationService = null!;

    [TestInitialize]
    public void Setup()
    {
        _context = SqliteTestDbContextFactory.CreateTestContext();

        _mapper = new ManaxMapper(new ManaxMapping());
        _mockNotificationService = new Mock<INotificationService>();

        _controller = new IssueController(_context, _mapper, _mockNotificationService.Object);

        ClaimsPrincipal user = new(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "TestUser1"),
            new Claim(ClaimTypes.Role, "User")
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
    public async Task GetAllAutomaticChapterIssues_ReturnsAllIssues()
    {
        ActionResult<IEnumerable<AutomaticIssueChapterDto>> result = await _controller.GetAllAutomaticChapterIssues();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        List<AutomaticIssueChapterDto>? returnedIssues = result.Value as List<AutomaticIssueChapterDto>;
        Assert.IsNotNull(returnedIssues);
        Assert.AreEqual(_context.AutomaticIssuesChapter.Count(), returnedIssues.Count);

        foreach (AutomaticIssueChapter issue in _context.AutomaticIssuesChapter)
        {
            AutomaticIssueChapterDto? returnedIssue = returnedIssues.FirstOrDefault(i => i.ChapterId == issue.ChapterId);
            Assert.IsNotNull(returnedIssue);
            Assert.AreEqual(issue.Problem, returnedIssue.Problem);
        }
    }

    [TestMethod]
    public async Task GetAllAutomaticSerieIssues_ReturnsAllIssues()
    {
        ActionResult<IEnumerable<AutomaticIssueSerieDto>> result = await _controller.GetAllAutomaticSerieIssues();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        List<AutomaticIssueSerieDto>? returnedIssues = result.Value as List<AutomaticIssueSerieDto>;
        Assert.IsNotNull(returnedIssues);
        Assert.AreEqual(_context.AutomaticIssuesSerie.Count(), returnedIssues.Count);

        foreach (AutomaticIssueSerie issue in _context.AutomaticIssuesSerie)
        {
            AutomaticIssueSerieDto? returnedIssue = returnedIssues.FirstOrDefault(i => i.SerieId == issue.SerieId);
            Assert.IsNotNull(returnedIssue);
            Assert.AreEqual(issue.Problem, returnedIssue.Problem);
        }
    }

    [TestMethod]
    public async Task GetAllReportedChapterIssues_ReturnsAllIssues()
    {
        ActionResult<IEnumerable<ReportedIssueChapterDto>> result = await _controller.GetAllReportedChapterIssues();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        List<ReportedIssueChapterDto>? returnedIssues = result.Value as List<ReportedIssueChapterDto>;
        Assert.IsNotNull(returnedIssues);
        Assert.AreEqual(_context.ReportedIssuesChapter.Count(), returnedIssues.Count);

        foreach (ReportedIssueChapter issue in _context.ReportedIssuesChapter)
        {
            ReportedIssueChapterDto? returnedIssue = returnedIssues.FirstOrDefault(i => i.Id == issue.Id);
            Assert.IsNotNull(returnedIssue);
            Assert.AreEqual(issue.ChapterId, returnedIssue.ChapterId);
            Assert.AreEqual(issue.UserId, returnedIssue.UserId);
            Assert.AreEqual(issue.ProblemId, returnedIssue.ProblemId);
        }
    }

    [TestMethod]
    public async Task GetAllReportedChapterIssuesTypes_ReturnsAllTypes()
    {
        List<ReportedIssueChapterTypeDto> result = await _controller.GetAllReportedChapterIssuesTypes();

        Assert.IsNotNull(result);
        Assert.AreEqual(_context.ReportedIssueChapterTypes.Count(), result.Count);

        foreach (ReportedIssueChapterType type in _context.ReportedIssueChapterTypes)
        {
            ReportedIssueChapterTypeDto? returnedType = result.FirstOrDefault(t => t.Id == type.Id);
            Assert.IsNotNull(returnedType);
            Assert.AreEqual(type.Name, returnedType.Name);
        }
    }

    [TestMethod]
    public async Task GetAllReportedSerieIssues_ReturnsAllIssues()
    {
        ActionResult<IEnumerable<ReportedIssueSerieDto>> result = await _controller.GetAllReportedSerieIssues();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        List<ReportedIssueSerieDto>? returnedIssues = result.Value as List<ReportedIssueSerieDto>;
        Assert.IsNotNull(returnedIssues);
        Assert.AreEqual(_context.ReportedIssuesSerie.Count(), returnedIssues.Count);

        foreach (ReportedIssueSerie issue in _context.ReportedIssuesSerie)
        {
            ReportedIssueSerieDto? returnedIssue = returnedIssues.FirstOrDefault(i => i.Id == issue.Id);
            Assert.IsNotNull(returnedIssue);
            Assert.AreEqual(issue.SerieId, returnedIssue.SerieId);
            Assert.AreEqual(issue.UserId, returnedIssue.UserId);
            Assert.AreEqual(issue.ProblemId, returnedIssue.ProblemId);
        }
    }

    [TestMethod]
    public async Task GetAllReportedSerieIssuesTypes_ReturnsAllTypes()
    {
        ActionResult<IEnumerable<ReportedIssueSerieType>> result = await _controller.GetAllReportedSerieIssuesTypes();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        List<ReportedIssueSerieType>? returnedTypes = result.Value as List<ReportedIssueSerieType>;
        Assert.IsNotNull(returnedTypes);
        Assert.AreEqual(_context.ReportedIssueSerieTypes.Count(), returnedTypes.Count);

        foreach (ReportedIssueSerieType type in _context.ReportedIssueSerieTypes)
        {
            ReportedIssueSerieType? returnedType = returnedTypes.FirstOrDefault(t => t.Id == type.Id);
            Assert.IsNotNull(returnedType);
            Assert.AreEqual(type.Name, returnedType.Name);
        }
    }

    [TestMethod]
    public async Task CreateChapterIssue_WithValidData_CreatesIssue()
    {
        ReportedIssueChapterCreateDto createDto = new()
        {
            ChapterId = 3,
            ProblemId = 1
        };

        ActionResult result = await _controller.CreateChapterIssue(createDto);

        Assert.IsInstanceOfType(result, typeof(CreatedResult));

        ReportedIssueChapter? createdIssue = _context.ReportedIssuesChapter
            .FirstOrDefault(i => i.ChapterId == createDto.ChapterId && i.UserId == 1 && i.ProblemId == createDto.ProblemId);
        Assert.IsNotNull(createdIssue);
        Assert.AreEqual(createDto.ChapterId, createdIssue.ChapterId);
        Assert.AreEqual(createDto.ProblemId, createdIssue.ProblemId);
        Assert.AreEqual(1, createdIssue.UserId);
    }

    [TestMethod]
    public async Task CreateChapterIssue_WithoutAuthentication_ReturnsUnauthorized()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        ReportedIssueChapterCreateDto createDto = new()
        {
            ChapterId = 1,
            ProblemId = 1
        };

        ActionResult result = await _controller.CreateChapterIssue(createDto);

        Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
    }

    [TestMethod]
    public async Task CreateChapterIssue_VerifyCreationDate()
    {
        ReportedIssueChapterCreateDto createDto = new()
        {
            ChapterId = 3,
            ProblemId = 2
        };

        DateTime before = DateTime.UtcNow;
        ActionResult result = await _controller.CreateChapterIssue(createDto);
        DateTime after = DateTime.UtcNow;

        Assert.IsInstanceOfType(result, typeof(CreatedResult));

        ReportedIssueChapter? createdIssue = _context.ReportedIssuesChapter
            .FirstOrDefault(i => i.ChapterId == createDto.ChapterId && i.UserId == 1);
        Assert.IsNotNull(createdIssue);
        Assert.IsTrue(createdIssue.CreatedAt >= before);
        Assert.IsTrue(createdIssue.CreatedAt <= after);
    }

    [TestMethod]
    public async Task CreateSerieIssue_WithValidData_CreatesIssue()
    {
        ReportedIssueSerieCreateDto createDto = new()
        {
            SerieId = 3,
            ProblemId = 1
        };

        ActionResult result = await _controller.CreateSerieIssue(createDto);

        Assert.IsInstanceOfType(result, typeof(CreatedResult));

        ReportedIssueSerie? createdIssue = _context.ReportedIssuesSerie
            .FirstOrDefault(i => i.SerieId == createDto.SerieId && i.ProblemId == createDto.ProblemId && i.UserId == 1);
        Assert.IsNotNull(createdIssue);
        Assert.AreEqual(createDto.SerieId, createdIssue.SerieId);
        Assert.AreEqual(createDto.ProblemId, createdIssue.ProblemId);
        Assert.AreEqual(1, createdIssue.UserId);
    }

    [TestMethod]
    public async Task CreateSerieIssue_WithoutAuthentication_ReturnsUnauthorized()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        ReportedIssueSerieCreateDto createDto = new()
        {
            SerieId = 1,
            ProblemId = 1
        };

        ActionResult result = await _controller.CreateSerieIssue(createDto);

        Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
    }

    [TestMethod]
    public async Task CreateSerieIssue_VerifyCreationDate()
    {
        ReportedIssueSerieCreateDto createDto = new()
        {
            SerieId = 3,
            ProblemId = 2
        };

        DateTime before = DateTime.UtcNow;
        ActionResult result = await _controller.CreateSerieIssue(createDto);
        DateTime after = DateTime.UtcNow;

        Assert.IsInstanceOfType(result, typeof(CreatedResult));

        ReportedIssueSerie? createdIssue = _context.ReportedIssuesSerie
            .FirstOrDefault(i => i.SerieId == createDto.SerieId && i.ProblemId == createDto.ProblemId && i.UserId == 1);
        Assert.IsNotNull(createdIssue);
        Assert.IsTrue(createdIssue.CreatedAt >= before);
        Assert.IsTrue(createdIssue.CreatedAt <= after);
    }

    [TestMethod]
    public async Task CloseChapterIssue_WithValidId_RemovesIssue()
    {
        ReportedIssueChapter issue = _context.ReportedIssuesChapter.First();
        IActionResult result = await _controller.CloseChapterIssue(issue.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        ReportedIssueChapter? deletedIssue = await _context.ReportedIssuesChapter.FindAsync(issue.Id);
        Assert.IsNull(deletedIssue);
    }

    [TestMethod]
    public async Task CloseChapterIssue_WithInvalidId_ReturnsNotFound()
    {
        IActionResult result = await _controller.CloseChapterIssue(999999);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task CloseChapterIssue_VerifyIssueCountDecreases()
    {
        int initialCount = _context.ReportedIssuesChapter.Count();
        ReportedIssueChapter issue = _context.ReportedIssuesChapter.First();

        IActionResult result = await _controller.CloseChapterIssue(issue.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        int finalCount = _context.ReportedIssuesChapter.Count();
        Assert.AreEqual(initialCount - 1, finalCount);
    }

    [TestMethod]
    public async Task CloseSerieIssue_WithValidId_RemovesIssue()
    {
        ReportedIssueSerie issue = _context.ReportedIssuesSerie.First();
        IActionResult result = await _controller.CloseSerieIssue(issue.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        ReportedIssueSerie? deletedIssue = await _context.ReportedIssuesSerie.FindAsync(issue.Id);
        Assert.IsNull(deletedIssue);
    }

    [TestMethod]
    public async Task CloseSerieIssue_WithInvalidId_ReturnsNotFound()
    {
        IActionResult result = await _controller.CloseSerieIssue(999999);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task CloseSerieIssue_VerifyIssueCountDecreases()
    {
        int initialCount = _context.ReportedIssuesSerie.Count();
        ReportedIssueSerie issue = _context.ReportedIssuesSerie.First();

        IActionResult result = await _controller.CloseSerieIssue(issue.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        int finalCount = _context.ReportedIssuesSerie.Count();
        Assert.AreEqual(initialCount - 1, finalCount);
    }

    [TestMethod]
    public async Task CreateChapterIssue_VerifyUserIdSetCorrectly()
    {
        ClaimsPrincipal user = new(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "2"),
            new Claim(ClaimTypes.Name, "TestAdmin"),
            new Claim(ClaimTypes.Role, "Admin")
        ], "test"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = user
            }
        };

        ReportedIssueChapterCreateDto createDto = new()
        {
            ChapterId = 1,
            ProblemId = 1
        };

        ActionResult result = await _controller.CreateChapterIssue(createDto);

        Assert.IsInstanceOfType(result, typeof(CreatedResult));

        ReportedIssueChapter? createdIssue = _context.ReportedIssuesChapter
            .FirstOrDefault(i => i.ChapterId == createDto.ChapterId && i.UserId == 2);
        Assert.IsNotNull(createdIssue);
        Assert.AreEqual(2, createdIssue.UserId);
    }

    [TestMethod]
    public async Task GetAllAutomaticChapterIssues_VerifyCorrectMapping()
    {
        ActionResult<IEnumerable<AutomaticIssueChapterDto>> result = await _controller.GetAllAutomaticChapterIssues();

        List<AutomaticIssueChapterDto>? returnedIssues = result.Value as List<AutomaticIssueChapterDto>;
        Assert.IsNotNull(returnedIssues);

        AutomaticIssueChapter expectedIssue = _context.AutomaticIssuesChapter.First();
        AutomaticIssueChapterDto? returnedIssue = returnedIssues.FirstOrDefault(i => i.ChapterId == expectedIssue.ChapterId);
        Assert.IsNotNull(returnedIssue);
        Assert.AreEqual(expectedIssue.ChapterId, returnedIssue.ChapterId);
        Assert.AreEqual(expectedIssue.Problem, returnedIssue.Problem);
        Assert.AreEqual(expectedIssue.CreatedAt, returnedIssue.CreatedAt);
    }

    [TestMethod]
    public async Task GetAllAutomaticSerieIssues_VerifyCorrectMapping()
    {
        ActionResult<IEnumerable<AutomaticIssueSerieDto>> result = await _controller.GetAllAutomaticSerieIssues();

        List<AutomaticIssueSerieDto>? returnedIssues = result.Value as List<AutomaticIssueSerieDto>;
        Assert.IsNotNull(returnedIssues);

        AutomaticIssueSerie expectedIssue = _context.AutomaticIssuesSerie.First();
        AutomaticIssueSerieDto? returnedIssue = returnedIssues.FirstOrDefault(i => i.SerieId == expectedIssue.SerieId);
        Assert.IsNotNull(returnedIssue);
        Assert.AreEqual(expectedIssue.SerieId, returnedIssue.SerieId);
        Assert.AreEqual(expectedIssue.Problem, returnedIssue.Problem);
        Assert.AreEqual(expectedIssue.CreatedAt, returnedIssue.CreatedAt);
    }

    [TestMethod]
    public async Task CreateChapterIssue_MultipleIssuesForSameChapter_CreatesMultipleIssues()
    {
        ReportedIssueChapterCreateDto firstDto = new()
        {
            ChapterId = 3,
            ProblemId = 1
        };

        ReportedIssueChapterCreateDto secondDto = new()
        {
            ChapterId = 3,
            ProblemId = 2
        };

        ActionResult firstResult = await _controller.CreateChapterIssue(firstDto);
        ActionResult secondResult = await _controller.CreateChapterIssue(secondDto);

        Assert.IsInstanceOfType(firstResult, typeof(CreatedResult));
        Assert.IsInstanceOfType(secondResult, typeof(CreatedResult));

        int issueCount = _context.ReportedIssuesChapter.Count(i => i.ChapterId == 3 && i.UserId == 1);
        Assert.AreEqual(2, issueCount);
    }

    [TestMethod]
    public async Task CreateSerieIssue_MultipleIssuesForSameSerie_CreatesMultipleIssues()
    {
        ReportedIssueSerieCreateDto firstDto = new()
        {
            SerieId = 3,
            ProblemId = 1
        };

        ReportedIssueSerieCreateDto secondDto = new()
        {
            SerieId = 3,
            ProblemId = 2
        };

        ActionResult firstResult = await _controller.CreateSerieIssue(firstDto);
        ActionResult secondResult = await _controller.CreateSerieIssue(secondDto);

        Assert.IsInstanceOfType(firstResult, typeof(CreatedResult));
        Assert.IsInstanceOfType(secondResult, typeof(CreatedResult));

        int issueCount = _context.ReportedIssuesSerie.Count(i => i.SerieId == 3 && i.UserId == 1);
        Assert.AreEqual(2, issueCount);
    }

    [TestMethod]
    public async Task CloseChapterIssue_OnlyRemovesSpecificIssue()
    {
        ReportedIssueChapter issueToDelete = _context.ReportedIssuesChapter.First();
        ReportedIssueChapter otherIssue = _context.ReportedIssuesChapter.First(i => i.Id != issueToDelete.Id);

        IActionResult result = await _controller.CloseChapterIssue(issueToDelete.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        ReportedIssueChapter? deletedIssue = await _context.ReportedIssuesChapter.FindAsync(issueToDelete.Id);
        Assert.IsNull(deletedIssue);

        ReportedIssueChapter? remainingIssue = await _context.ReportedIssuesChapter.FindAsync(otherIssue.Id);
        Assert.IsNotNull(remainingIssue);
    }

    [TestMethod]
    public async Task CloseSerieIssue_OnlyRemovesSpecificIssue()
    {
        ReportedIssueSerie issueToDelete = _context.ReportedIssuesSerie.First();
        ReportedIssueSerie otherIssue = _context.ReportedIssuesSerie.First(i => i.Id != issueToDelete.Id);

        IActionResult result = await _controller.CloseSerieIssue(issueToDelete.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        ReportedIssueSerie? deletedIssue = await _context.ReportedIssuesSerie.FindAsync(issueToDelete.Id);
        Assert.IsNull(deletedIssue);

        ReportedIssueSerie? remainingIssue = await _context.ReportedIssuesSerie.FindAsync(otherIssue.Id);
        Assert.IsNotNull(remainingIssue);
    }

    [TestMethod]
    public async Task GetAllReportedChapterIssuesTypes_VerifyCorrectCount()
    {
        List<ReportedIssueChapterTypeDto> result = await _controller.GetAllReportedChapterIssuesTypes();

        Assert.IsNotNull(result);
        Assert.HasCount(3, result);
    }

    [TestMethod]
    public async Task GetAllReportedSerieIssuesTypes_VerifyCorrectCount()
    {
        ActionResult<IEnumerable<ReportedIssueSerieType>> result = await _controller.GetAllReportedSerieIssuesTypes();

        List<ReportedIssueSerieType>? returnedTypes = result.Value as List<ReportedIssueSerieType>;
        Assert.IsNotNull(returnedTypes);
        Assert.AreEqual(3, returnedTypes.Count);
    }

    [TestMethod]
    public async Task CreateChapterIssue_WithDuplicateData_ReturnsConflict()
    {
        ReportedIssueChapterCreateDto createDto = new()
        {
            ChapterId = 3,
            ProblemId = 3
        };

        ActionResult firstResult = await _controller.CreateChapterIssue(createDto);
        ActionResult secondResult = await _controller.CreateChapterIssue(createDto);

        Assert.IsInstanceOfType(firstResult, typeof(CreatedResult));
        Assert.IsInstanceOfType(secondResult, typeof(ConflictObjectResult));
    }

    [TestMethod]
    public async Task CreateSerieIssue_WithDuplicateData_ReturnsConflict()
    {
        ReportedIssueSerieCreateDto createDto = new()
        {
            SerieId = 3,
            ProblemId = 3
        };

        ActionResult firstResult = await _controller.CreateSerieIssue(createDto);
        ActionResult secondResult = await _controller.CreateSerieIssue(createDto);

        Assert.IsInstanceOfType(firstResult, typeof(CreatedResult));
        Assert.IsInstanceOfType(secondResult, typeof(ConflictObjectResult));
    }
}
