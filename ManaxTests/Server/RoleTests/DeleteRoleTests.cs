using ManaxLibrary;
using ManaxServer.Models.Person;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.Server.RoleTests;

[TestClass]
public class DeleteRoleTests : RoleTestsSetup
{
    [TestMethod]
    public async Task DeleteRoleWithValidIdDeletesRole()
    {
        Role role = new() { Name = "ToDelete" };
        Context.Roles.Add(role);
        await Context.SaveChangesAsync();
        int initialCount = Context.Roles.Count();

        ActionResult result = await Controller.DeleteRole(role.Id);

        Assert.IsInstanceOfType<OkResult>(result);

        Role? deletedRole = await Context.Roles.FindAsync(role.Id);
        Assert.IsNull(deletedRole);
        Assert.AreEqual(initialCount - 1, Context.Roles.Count());
    }

    [TestMethod]
    public async Task DeleteRoleWithNonExistentIdReturnsNotFound()
    {
        ActionResult result = await Controller.DeleteRole(999999);

        CheckTypeAndErrorCode<NotFoundObjectResult>(result, ErrorCode.RoleDoesNotExist);
    }

    [TestMethod]
    public async Task DeleteRoleVerifyNotificationIsSent()
    {
        Role role = new() { Name = "NotifyDelete" };
        Context.Roles.Add(role);
        await Context.SaveChangesAsync();

        long roleId = role.Id;

        ActionResult result = await Controller.DeleteRole(roleId);

        Assert.IsInstanceOfType<OkResult>(result);
        Assert.AreEqual(roleId, MockNotificationService.RoleDeletedId);
    }
}
