using ManaxLibrary;
using ManaxLibrary.DTO.Role;
using ManaxServer.Models.Person;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.Server.RoleTests;

[TestClass]
public class UpdateRoleTests : RoleTestsSetup
{
    [TestMethod]
    public async Task UpdateRoleWithValidDataUpdatesExistingRole()
    {
        Role role = new() { Name = "Author" };
        Context.Roles.Add(role);
        await Context.SaveChangesAsync();

        RoleUpdateDto roleUpdate = new()
        {
            Name = "Updated Author"
        };

        ActionResult result = await Controller.UpdateRole(role.Id, roleUpdate);

        Assert.IsInstanceOfType<OkResult>(result);

        Role? updatedRole = await Context.Roles.FindAsync(role.Id);
        Assert.IsNotNull(updatedRole);
        Assert.AreEqual(roleUpdate.Name, updatedRole.Name);
    }

    [TestMethod]
    public async Task UpdateRoleWithNonExistentIdReturnsNotFound()
    {
        RoleUpdateDto roleUpdate = new()
        {
            Name = "Non Existent"
        };

        ActionResult result = await Controller.UpdateRole(999999, roleUpdate);

        CheckTypeAndErrorCode<NotFoundObjectResult>(result, ErrorCode.RoleDoesNotExist);
    }

    [TestMethod]
    public async Task UpdateRoleVerifyNotificationIsSent()
    {
        Role role = new() { Name = "Artist" };
        Context.Roles.Add(role);
        await Context.SaveChangesAsync();

        RoleUpdateDto roleUpdate = new()
        {
            Name = "Updated Artist"
        };

        await Controller.UpdateRole(role.Id, roleUpdate);

        Assert.IsNotNull(MockNotificationService.RoleUpdated);
        Assert.AreEqual(roleUpdate.Name, MockNotificationService.RoleUpdated.Name);
    }

    [TestMethod]
    public async Task UpdateRoleWithEmptyNameDoesNotUpdatesRole()
    {
        Role role = new() { Name = "Translator" };
        Context.Roles.Add(role);
        await Context.SaveChangesAsync();

        RoleUpdateDto roleUpdate = new()
        {
            Name = ""
        };

        ActionResult result = await Controller.UpdateRole(role.Id, roleUpdate);

        CheckTypeAndErrorCode<BadRequestObjectResult>(result, ErrorCode.InvalidRoleData);
    }
}