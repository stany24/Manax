using System.Security.Claims;
using ManaxLibrary.DTO.User;
using ManaxServer.Controllers;
using ManaxServer.Models;
using ManaxServer.Models.Claim;
using ManaxServer.Models.User;
using ManaxServer.Services.Mapper;
using ManaxServer.Services.Notification;
using ManaxTests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ManaxTests;

[TestClass]
public class TestUserController
{
    private ManaxContext _context = null!;
    private UserController _controller = null!;
    private ManaxMapper _mapper = null!;
    private MockHashService _mockHashService = null!;
    private MockJwtService _mockJwtService = null!;
    private Mock<INotificationService> _mockNotificationService = null!;

    [TestInitialize]
    public void Setup()
    {
        _context = SqliteTestDbContextFactory.CreateTestContext();

        _mapper = new ManaxMapper(new ManaxMapping());
        _mockHashService = new MockHashService();
        _mockJwtService = new MockJwtService();
        _mockNotificationService = new Mock<INotificationService>();

        _controller = new UserController(_context, _mapper, _mockHashService, _mockJwtService, _mockNotificationService.Object);

        ClaimsPrincipal adminUser = new(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "2"),
            new Claim(ClaimTypes.Name, "TestAdmin"),
            new Claim(ClaimTypes.Role, "Admin")
        ], "test"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = adminUser
            }
        };
    }

    [TestCleanup]
    public void Cleanup()
    {
        SqliteTestDbContextFactory.CleanupTestDatabase(_context);
        _mockHashService.Reset();
    }

    [TestMethod]
    public async Task GetUsers_ReturnsAllUserIds()
    {
        ActionResult<IEnumerable<long>> result = await _controller.GetUsers();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        List<long>? returnedIds = result.Value as List<long>;
        Assert.IsNotNull(returnedIds);
        Assert.AreEqual(_context.Users.Count(), returnedIds.Count);
        foreach (User user in _context.Users) Assert.Contains(user.Id, returnedIds);
    }

    [TestMethod]
    public async Task GetUser_WithValidId_ReturnsUser()
    {
        User user = _context.Users.First();
        ActionResult<UserDto> result = await _controller.GetUser(user.Id);

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        UserDto? returnedUser = result.Value;
        Assert.IsNotNull(returnedUser);
        Assert.AreEqual(user.Id, returnedUser.Id);
        Assert.AreEqual(user.Username, returnedUser.Username);
        Assert.AreEqual(user.Role, returnedUser.Role);
    }

    [TestMethod]
    public async Task GetUser_WithInvalidId_ReturnsNotFound()
    {
        ActionResult<UserDto> result = await _controller.GetUser(999999);

        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task PutUser_WithValidId_UpdatesUser()
    {
        User user = _context.Users.First();
        UserUpdateDto updateDto = new()
        {
            Username = "UpdatedUsername",
            Role = UserRole.Admin,
            Password = "newPassword"
        };

        IActionResult result = await _controller.PutUser(user.Id, updateDto);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        User? updatedUser = await _context.Users.FindAsync(user.Id);
        Assert.IsNotNull(updatedUser);
        Assert.AreEqual(updateDto.Username, updatedUser.Username);
        Assert.AreEqual(updateDto.Role, updatedUser.Role);
        _mockHashService.VerifyHashPasswordCalled("newPassword");
    }

    [TestMethod]
    public async Task PutUser_WithInvalidId_ReturnsNotFound()
    {
        UserUpdateDto updateDto = new()
        {
            Username = "UpdatedUsername",
            Role = UserRole.Admin,
            Password = "newPassword"
        };

        IActionResult result = await _controller.PutUser(999999, updateDto);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task PutUser_WithoutPassword_DoesNotUpdatePassword()
    {
        User user = _context.Users.First();
        string originalPasswordHash = user.PasswordHash;
        UserUpdateDto updateDto = new()
        {
            Username = "UpdatedUsername",
            Role = UserRole.Admin,
            Password = ""
        };

        IActionResult result = await _controller.PutUser(user.Id, updateDto);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        User? updatedUser = await _context.Users.FindAsync(user.Id);
        Assert.IsNotNull(updatedUser);
        Assert.AreEqual(originalPasswordHash, updatedUser.PasswordHash);
        _mockHashService.VerifyHashPasswordNotCalled();
    }

    [TestMethod]
    public async Task PostUser_WithValidData_CreatesUser()
    {
        UserCreateDto createDto = new()
        {
            Username = "NewUser",
            Password = "password123",
            Role = UserRole.User
        };

        ActionResult<long> result = await _controller.PostUser(createDto);

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        long? userId = result.Value;
        Assert.IsNotNull(userId);

        User? createdUser = await _context.Users.FindAsync(userId);
        Assert.IsNotNull(createdUser);
        Assert.AreEqual(createDto.Username, createdUser.Username);
        Assert.AreEqual(createDto.Role, createdUser.Role);
        _mockHashService.VerifyHashPasswordCalled("password123");
        _mockNotificationService.Verify(x => x.NotifyUserCreatedAsync(It.IsAny<UserDto>()), Times.Once);
    }

    [TestMethod]
    public async Task PostUser_CreationDateSetCorrectly()
    {
        UserCreateDto createDto = new()
        {
            Username = "NewUserWithDate",
            Password = "password123",
            Role = UserRole.User
        };

        DateTime beforeCreation = DateTime.UtcNow;
        ActionResult<long> result = await _controller.PostUser(createDto);
        DateTime afterCreation = DateTime.UtcNow;

        long? userId = result.Value;
        Assert.IsNotNull(userId);

        User? createdUser = await _context.Users.FindAsync(userId);
        Assert.IsNotNull(createdUser);
        Assert.IsTrue(createdUser.Creation >= beforeCreation);
        Assert.IsTrue(createdUser.Creation <= afterCreation);
    }

    [TestMethod]
    public async Task DeleteUser_WithValidId_RemovesUser()
    {
        User user = _context.Users.First();
        IActionResult result = await _controller.DeleteUser(user.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        User? deletedUser = await _context.Users.FindAsync(user.Id);
        Assert.IsNull(deletedUser);
        _mockNotificationService.Verify(x => x.NotifyUserDeletedAsync(user.Id), Times.Once);
    }

    [TestMethod]
    public async Task DeleteUser_WithInvalidId_ReturnsNotFound()
    {
        IActionResult result = await _controller.DeleteUser(999999);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task DeleteUser_TryingToDeleteSelf_ReturnsForbid()
    {
        ClaimsPrincipal selfUser = new(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "TestUser1"),
            new Claim(ClaimTypes.Role, "User")
        ], "test"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = selfUser
            }
        };

        IActionResult result = await _controller.DeleteUser(1);

        Assert.IsInstanceOfType(result, typeof(ForbidResult));
    }

    [TestMethod]
    public async Task DeleteUser_AdminTryingToDeleteOwner_ReturnsForbid()
    {
        User ownerUser = new()
        {
            Id = 100,
            Username = "OwnerUser",
            Role = UserRole.Owner,
            Creation = DateTime.UtcNow
        };
        _context.Users.Add(ownerUser);
        await _context.SaveChangesAsync();

        IActionResult result = await _controller.DeleteUser(100);

        Assert.IsInstanceOfType(result, typeof(ForbidResult));
    }

    [TestMethod]
    public async Task DeleteUser_AdminTryingToDeleteAnotherAdmin_ReturnsForbid()
    {
        User anotherAdmin = new()
        {
            Id = 101,
            Username = "AnotherAdmin",
            Role = UserRole.Admin,
            Creation = DateTime.UtcNow
        };
        _context.Users.Add(anotherAdmin);
        await _context.SaveChangesAsync();

        IActionResult result = await _controller.DeleteUser(101);

        Assert.IsInstanceOfType(result, typeof(ForbidResult));
    }

    [TestMethod]
    public async Task DeleteUser_WithoutAuthentication_ReturnsUnauthorized()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        IActionResult result = await _controller.DeleteUser(1);

        Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
    }

    [TestMethod]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        User user = _context.Users.First();
        user.PasswordHash = "correctPasswordhashed";
        await _context.SaveChangesAsync();
        
        UserLoginDto loginDto = new()
        {
            Username = user.Username,
            Password = "correctPassword"
        };

        ActionResult<UserLoginResultDto> result = await _controller.Login(loginDto);

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        UserLoginResultDto? loginResult = result.Value;
        Assert.IsNotNull(loginResult);
        Assert.AreEqual($"{user.Id}-jwt-token", loginResult.Token);
        Assert.IsNotNull(loginResult.User);
        Assert.AreEqual(user.Id, loginResult.User.Id);

        _mockHashService.VerifyVerifyPasswordCalled("correctPassword");
    }

    [TestMethod]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        UserLoginDto loginDto = new()
        {
            Username = "testuser",
            Password = "wrongPassword"
        };

        ActionResult<UserLoginResultDto> result = await _controller.Login(loginDto);

        Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedObjectResult));

        LoginAttempt? loginAttempt = _context.LoginAttempts.FirstOrDefault(la => la.Username == "testuser");
        Assert.IsNotNull(loginAttempt);
        Assert.IsFalse(loginAttempt.Success);
        Assert.AreEqual("Login", loginAttempt.Type);
    }

    [TestMethod]
    public async Task Login_WithNonExistentUser_ReturnsUnauthorized()
    {
        UserLoginDto loginDto = new()
        {
            Username = "nonexistent",
            Password = "password"
        };

        ActionResult<UserLoginResultDto> result = await _controller.Login(loginDto);

        Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedObjectResult));

        LoginAttempt? loginAttempt = _context.LoginAttempts.FirstOrDefault(la => la.Username == "nonexistent");
        Assert.IsNotNull(loginAttempt);
        Assert.IsFalse(loginAttempt.Success);
    }

    [TestMethod]
    public async Task Login_UpdatesLastLoginTime()
    {
        User user = _context.Users.First();
        user.PasswordHash = "correctPasswordhashed";
        await _context.SaveChangesAsync();
        
        DateTime originalLastLogin = user.LastLogin;
        UserLoginDto loginDto = new()
        {
            Username = user.Username,
            Password = "correctPassword"
        };

        await Task.Delay(10);

        ActionResult<UserLoginResultDto> result = await _controller.Login(loginDto);

        UserLoginResultDto? loginResult = result.Value;
        Assert.IsNotNull(loginResult);

        User? updatedUser = await _context.Users.FindAsync(user.Id);
        Assert.IsNotNull(updatedUser);
        Assert.IsTrue(updatedUser.LastLogin > originalLastLogin);
    }

    [TestMethod]
    public async Task Login_CreatesSuccessfulLoginAttempt()
    {
        User user = _context.Users.First();
        user.PasswordHash = "correctPasswordhashed";
        await _context.SaveChangesAsync();
        
        UserLoginDto loginDto = new()
        {
            Username = user.Username,
            Password = "correctPassword"
        };

        ActionResult<UserLoginResultDto> result = await _controller.Login(loginDto);

        UserLoginResultDto? loginResult = result.Value;
        Assert.IsNotNull(loginResult);

        LoginAttempt? loginAttempt = _context.LoginAttempts.FirstOrDefault(la => la.Username == user.Username);
        Assert.IsNotNull(loginAttempt);
        Assert.IsTrue(loginAttempt.Success);
        Assert.AreEqual("Login", loginAttempt.Type);
    }

    [TestMethod]
    public async Task Claim_WithNoExistingUsers_CreatesOwnerUser()
    {
        _context.Users.RemoveRange(_context.Users);
        await _context.SaveChangesAsync();

        ClaimRequest claimRequest = new()
        {
            Username = "FirstOwner",
            Password = "ownerPassword"
        };

        ActionResult<UserLoginResultDto> result = _controller.Claim(claimRequest);

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        UserLoginResultDto? claimResult = result.Value;
        Assert.IsNotNull(claimResult);
        
        User? createdUser = _context.Users.FirstOrDefault(u => u.Username == "FirstOwner");
        Assert.IsNotNull(createdUser);
        Assert.AreEqual($"{createdUser.Id}-jwt-token", claimResult.Token);
        Assert.IsNotNull(claimResult.User);
        Assert.AreEqual(UserRole.Owner, claimResult.User.Role);
        Assert.AreEqual(UserRole.Owner, createdUser.Role);
    }

    [TestMethod]
    public Task Claim_WithExistingUsers_ReturnsUnauthorized()
    {
        ClaimRequest claimRequest = new()
        {
            Username = "SomeUser",
            Password = "password"
        };

        ActionResult<UserLoginResultDto> result = _controller.Claim(claimRequest);

        Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedObjectResult));

        LoginAttempt? claimAttempt = _context.LoginAttempts.FirstOrDefault(la => la.Type == "Claim");
        Assert.IsNotNull(claimAttempt);
        Assert.IsFalse(claimAttempt.Success);
        return Task.CompletedTask;
    }

    [TestMethod]
    public async Task Claim_CreatesClaimLoginAttempt()
    {
        _context.Users.RemoveRange(_context.Users);
        await _context.SaveChangesAsync();

        ClaimRequest claimRequest = new()
        {
            Username = "FirstOwner",
            Password = "ownerPassword"
        };

        ActionResult<UserLoginResultDto> result = _controller.Claim(claimRequest);

        UserLoginResultDto? claimResult = result.Value;
        Assert.IsNotNull(claimResult);

        LoginAttempt? claimAttempt = _context.LoginAttempts.FirstOrDefault(la => la.Type == "Claim");
        Assert.IsNotNull(claimAttempt);
        Assert.IsTrue(claimAttempt.Success);
        Assert.AreEqual("FirstOwner", claimAttempt.Username);
    }

    [TestMethod]
    public async Task DeleteUser_VerifyUserCountDecreases()
    {
        int initialCount = _context.Users.Count();
        User user = _context.Users.First();

        IActionResult result = await _controller.DeleteUser(user.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        int finalCount = _context.Users.Count();
        Assert.AreEqual(initialCount - 1, finalCount);
    }

    [TestMethod]
    public async Task PostUser_VerifyUserCountIncreases()
    {
        int initialCount = _context.Users.Count();
        UserCreateDto createDto = new()
        {
            Username = "CountTestUser",
            Password = "password123",
            Role = UserRole.User
        };

        ActionResult<long> result = await _controller.PostUser(createDto);

        long? userId = result.Value;
        Assert.IsNotNull(userId);

        int finalCount = _context.Users.Count();
        Assert.AreEqual(initialCount + 1, finalCount);
    }

    [TestMethod]
    public async Task GetUser_VerifyAllPropertiesMapping()
    {
        User user = _context.Users.First();
        ActionResult<UserDto> result = await _controller.GetUser(user.Id);

        UserDto? returnedUser = result.Value;
        Assert.IsNotNull(returnedUser);
        Assert.AreEqual(user.Id, returnedUser.Id);
        Assert.AreEqual(user.Username, returnedUser.Username);
        Assert.AreEqual(user.Role, returnedUser.Role);
        Assert.AreEqual(user.LastLogin, returnedUser.LastLogin);
    }

    [TestMethod]
    public async Task PutUser_WithEmptyPassword_DoesNotCallHashService()
    {
        User user = _context.Users.First();
        UserUpdateDto updateDto = new()
        {
            Username = "UpdatedUser",
            Role = UserRole.User,
            Password = ""
        };

        IActionResult result = await _controller.PutUser(user.Id, updateDto);

        Assert.IsInstanceOfType(result, typeof(OkResult));
        _mockHashService.VerifyHashPasswordNotCalled();
    }

    [TestMethod]
    public async Task PutUser_WithNullPassword_DoesNotCallHashService()
    {
        User user = _context.Users.First();
        UserUpdateDto updateDto = new()
        {
            Username = "UpdatedUser",
            Role = UserRole.User,
            Password = null!
        };

        IActionResult result = await _controller.PutUser(user.Id, updateDto);

        Assert.IsInstanceOfType(result, typeof(OkResult));
        _mockHashService.VerifyHashPasswordNotCalled();
    }

    [TestMethod]
    public async Task Login_WithEmptyDatabase_RedirectsToClaim()
    {
        _context.Users.RemoveRange(_context.Users);
        await _context.SaveChangesAsync();

        UserLoginDto loginDto = new()
        {
            Username = "testuser",
            Password = "password"
        };

        ActionResult<UserLoginResultDto> result = await _controller.Login(loginDto);

        UserLoginResultDto? loginResult = result.Value;
        Assert.IsNotNull(loginResult);
        
        User? createdUser = _context.Users.FirstOrDefault(u => u.Username == "testuser");
        Assert.IsNotNull(createdUser);
        Assert.AreEqual($"{createdUser.Id}-jwt-token", loginResult.Token);
        Assert.AreEqual(UserRole.Owner, loginResult.User.Role);
    }

    [TestMethod]
    public async Task DeleteUser_WithNonExistentCurrentUser_ReturnsUnauthorized()
    {
        ClaimsPrincipal nonExistentUser = new(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "999"),
            new Claim(ClaimTypes.Name, "NonExistent"),
            new Claim(ClaimTypes.Role, "Admin")
        ], "test"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = nonExistentUser
            }
        };

        IActionResult result = await _controller.DeleteUser(1);

        Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
    }

    [TestMethod]
    public async Task PostUser_VerifyNotificationCalledWithCorrectData()
    {
        UserCreateDto createDto = new()
        {
            Username = "NotificationTestUser",
            Password = "password123",
            Role = UserRole.User
        };

        ActionResult<long> result = await _controller.PostUser(createDto);

        long? userId = result.Value;
        Assert.IsNotNull(userId);

        _mockNotificationService.Verify(x => x.NotifyUserCreatedAsync(It.Is<UserDto>(dto =>
            dto.Username == "NotificationTestUser" &&
            dto.Role == UserRole.User
        )), Times.Once);
    }

    [TestMethod]
    public async Task Login_VerifyLoginAttemptHasCorrectOrigin()
    {
        User user = _context.Users.First();
        user.PasswordHash = "correctPasswordhashed";
        await _context.SaveChangesAsync();
        
        UserLoginDto loginDto = new()
        {
            Username = user.Username,
            Password = "correctPassword"
        };

        _controller.ControllerContext.HttpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

        ActionResult<UserLoginResultDto> result = await _controller.Login(loginDto);

        UserLoginResultDto? loginResult = result.Value;
        Assert.IsNotNull(loginResult);

        LoginAttempt? loginAttempt = _context.LoginAttempts.FirstOrDefault(la => la.Username == user.Username);
        Assert.IsNotNull(loginAttempt);
        Assert.AreEqual("127.0.0.1", loginAttempt.Origin);
    }

    [TestMethod]
    public async Task DeleteUser_OwnerCanDeleteAdmin()
    {
        ClaimsPrincipal ownerUser = new(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "3"),
            new Claim(ClaimTypes.Name, "OwnerUser"),
            new Claim(ClaimTypes.Role, "Owner")
        ], "test"));

        User ownerUserEntity = new()
        {
            Id = 3,
            Username = "OwnerUser",
            Role = UserRole.Owner,
            Creation = DateTime.UtcNow
        };
        _context.Users.Add(ownerUserEntity);
        await _context.SaveChangesAsync();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = ownerUser
            }
        };

        User adminUser = _context.Users.First(u => u.Role == UserRole.Admin);
        IActionResult result = await _controller.DeleteUser(adminUser.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));
    }

    [TestMethod]
    public async Task Claim_VerifyPasswordIsHashed()
    {
        _context.Users.RemoveRange(_context.Users);
        await _context.SaveChangesAsync();

        ClaimRequest claimRequest = new()
        {
            Username = "HashedPasswordTest",
            Password = "plainTextPassword"
        };

        ActionResult<UserLoginResultDto> result = _controller.Claim(claimRequest);

        UserLoginResultDto? claimResult = result.Value;
        Assert.IsNotNull(claimResult);

        _mockHashService.VerifyHashPasswordCalled("plainTextPassword");

        User? createdUser = _context.Users.FirstOrDefault(u => u.Username == "HashedPasswordTest");
        Assert.IsNotNull(createdUser);
        Assert.AreEqual("plainTextPasswordhashed", createdUser.PasswordHash);
    }
}
