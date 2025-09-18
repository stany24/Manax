using System.Security.Claims;
using ManaxLibrary.DTO.User;
using ManaxServer.Models.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.Server.UserTests;

[TestClass]
public class DeleteUserTests : UserTestsSetup
{
    [TestMethod]
    public async Task DeleteUserWithValidIdRemovesUser()
    {
        User user = Context.Users.First();
        IActionResult result = await Controller.DeleteUser(user.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        User? deletedUser = await Context.Users.FindAsync(user.Id);
        Assert.IsNull(deletedUser);
    }

    [TestMethod]
    public async Task DeleteUserWithInvalidIdReturnsNotFound()
    {
        IActionResult result = await Controller.DeleteUser(999999);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task DeleteUserTryingToDeleteSelfReturnsForbid()
    {
        ClaimsPrincipal selfUser = new(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "TestUser1"),
            new Claim(ClaimTypes.Role, "User")
        ], "test"));

        Controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = selfUser
            }
        };

        IActionResult result = await Controller.DeleteUser(1);

        Assert.IsInstanceOfType(result, typeof(ForbidResult));
    }

    [TestMethod]
    public async Task DeleteUserAdminTryingToDeleteOwnerReturnsForbid()
    {
        User ownerUser = new()
        {
            Id = 100,
            Username = "OwnerUser",
            Role = UserRole.Owner,
            Creation = DateTime.UtcNow
        };
        Context.Users.Add(ownerUser);
        await Context.SaveChangesAsync();

        IActionResult result = await Controller.DeleteUser(100);

        Assert.IsInstanceOfType(result, typeof(ForbidResult));
    }

    [TestMethod]
    public async Task DeleteUserAdminTryingToDeleteAnotherAdminReturnsForbid()
    {
        User anotherAdmin = new()
        {
            Id = 101,
            Username = "AnotherAdmin",
            Role = UserRole.Admin,
            Creation = DateTime.UtcNow
        };
        Context.Users.Add(anotherAdmin);
        await Context.SaveChangesAsync();

        IActionResult result = await Controller.DeleteUser(101);

        Assert.IsInstanceOfType(result, typeof(ForbidResult));
    }

    [TestMethod]
    public async Task DeleteUserWithoutAuthenticationReturnsUnauthorized()
    {
        Controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        IActionResult result = await Controller.DeleteUser(1);

        Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
    }

    [TestMethod]
    public async Task DeleteUserVerifyUserCountDecreases()
    {
        int initialCount = Context.Users.Count();
        User user = Context.Users.First();

        IActionResult result = await Controller.DeleteUser(user.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        int finalCount = Context.Users.Count();
        Assert.AreEqual(initialCount - 1, finalCount);
    }

    [TestMethod]
    public async Task DeleteUserWithNonExistentCurrentUserReturnsUnauthorized()
    {
        ClaimsPrincipal nonExistentUser = new(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "999"),
            new Claim(ClaimTypes.Name, "NonExistent"),
            new Claim(ClaimTypes.Role, "Admin")
        ], "test"));

        Controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = nonExistentUser
            }
        };

        IActionResult result = await Controller.DeleteUser(1);

        Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
    }

    [TestMethod]
    public async Task DeleteUserOwnerCanDeleteAdmin()
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
        Context.Users.Add(ownerUserEntity);
        await Context.SaveChangesAsync();

        Controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = ownerUser
            }
        };

        User adminUser = Context.Users.First(u => u.Role == UserRole.Admin);
        IActionResult result = await Controller.DeleteUser(adminUser.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));
    }
}