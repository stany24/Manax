using System.Reflection;
using ManaxClient.Manager;
using ManaxLibrary.DTO.User;

namespace ManaxTests.Client;

[TestClass]
public class VerificationTests
{
    [TestMethod]
    public void VerifyAllPermissionProperties()
    {
        Type type = typeof(PermissionManager);
        List<PropertyInfo> propertyInfos = type.GetProperties().ToList();
        List<Permission> allPermissions = Enum.GetValues<Permission>().ToList();
        foreach (Permission permission in allPermissions)
        {
            string propertyName = "Can" + permission;
            if (!propertyInfos.Any(p => p.Name == propertyName && p.PropertyType == typeof(bool)))
                Assert.Fail($"Missing permission property: {propertyName} in {type}");
        }
    }
}