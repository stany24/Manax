using ManaxLibrary.DTO.Role;
using ManaxServer.Models.Person;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxTests.Server.RoleTests;

[TestClass]
public class CreateRoleTests : RoleTestsSetup
{
    private readonly RoleCreateDto _roleCreate = new()
    {
        Name = "Author"
    };
    
    [TestMethod]
    public async Task CreateRoleWithValidDataCreatesNewRole()
    {
        int initialCount = Context.Roles.Count();
        ActionResult result = await Controller.CreateRole(_roleCreate);

        Assert.IsInstanceOfType<OkObjectResult>(result);

        Role? createdRole = await Context.Roles.FirstOrDefaultAsync(r => r.Name == _roleCreate.Name);
        Assert.IsNotNull(createdRole);
        Assert.AreEqual(_roleCreate.Name, createdRole.Name);
        Assert.AreEqual(initialCount + 1, Context.Roles.Count());
    }

    [TestMethod]
    public async Task CreateRoleVerifyNotificationIsSent()
    {
        await Controller.CreateRole(_roleCreate);

        Assert.IsNotNull(MockNotificationService.RoleCreated);
        Assert.AreEqual(_roleCreate.Name, MockNotificationService.RoleCreated.Name);
    }

    [TestMethod]
    public async Task CreateRoleReturnsCreatedRoleId()
    {
        ActionResult result = await Controller.CreateRole(_roleCreate);

        Assert.IsInstanceOfType<OkObjectResult>(result);
        OkObjectResult okResult = (OkObjectResult)result;
        Assert.IsNotNull(okResult.Value);
        Assert.IsInstanceOfType<long>(okResult.Value);

        long returnedId = (long)okResult.Value;
        Role? createdRole = await Context.Roles.FindAsync(returnedId);
        Assert.IsNotNull(createdRole);
        Assert.AreEqual(_roleCreate.Name, createdRole.Name);
    }

    [TestMethod]
    public async Task CreateMultipleRolesWithDifferentNamesSucceeds()
    {
        RoleCreateDto roleCreate1 = new() { Name = "Writer" };
        RoleCreateDto roleCreate2 = new() { Name = "Illustrator" };
        RoleCreateDto roleCreate3 = new() { Name = "Translator" };

        ActionResult result1 = await Controller.CreateRole(roleCreate1);
        ActionResult result2 = await Controller.CreateRole(roleCreate2);
        ActionResult result3 = await Controller.CreateRole(roleCreate3);

        Assert.IsInstanceOfType<OkObjectResult>(result1);
        Assert.IsInstanceOfType<OkObjectResult>(result2);
        Assert.IsInstanceOfType<OkObjectResult>(result3);

        Assert.AreEqual(3, Context.Roles.Count());
    }

    [TestMethod]
    public async Task CreateRoleWithEmptyNameDoesNotCreatesRole()
    {
        ActionResult result = await Controller.CreateRole(_roleCreate);

        Assert.IsInstanceOfType<OkObjectResult>(result);

        Role? createdRole = await Context.Roles.FirstOrDefaultAsync(r => r.Name == "");
        Assert.IsNull(createdRole);
    }
}
