using ManaxLibrary.DTO.User;
using ManaxServer.Controllers;

namespace ManaxTests.Server;

[TestClass]
public class VerificationTests
{
    [TestMethod]
    public void VerifyPermissions()
    {
        Permission[] ownerPermissions = PermissionController.GetDefaultPermissionsForRole(UserRole.Owner);
        List<Permission> allPermissions = Enum.GetValues<Permission>().ToList();
        foreach (Permission permission in allPermissions.Where(permission => !ownerPermissions.Contains(permission)))
            Assert.Fail($"Owner role is missing permission: {permission}");
    }
}