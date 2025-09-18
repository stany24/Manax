using ManaxLibrary.DTO.User;
using ManaxServer.Controllers;
using ManaxServer.Localization;
using ManaxServer.Localization.Languages;

namespace ManaxTests.Server;
[TestClass]
public class VerificationTests
{
    [TestMethod]
    public void VerifyLocalizations()
    {
        VerifyLocalizationKeys(new FrenchLocalization().GetLocalization());
        VerifyLocalizationKeys(new EnglishLocalization().GetLocalization());
    }
    
    private static void VerifyLocalizationKeys(Dictionary<LocalizationKey, string> localization)
    {
        List<LocalizationKey> keys = Enum.GetValues(typeof(LocalizationKey)).Cast<LocalizationKey>().ToList();
        foreach (LocalizationKey key in keys.Where(key => !localization.ContainsKey(key)))
            throw new Exception($"Missing localization for key: {key}");

        if (localization.Count != keys.Count) throw new Exception("Localization contains extra keys");
    }
    
    [TestMethod]
    public void VerifyPermissions()
    {
        Permission[] ownerPermissions = PermissionController.GetDefaultPermissionsForRole(UserRole.Owner);
        List<Permission> allPermissions = Enum.GetValues(typeof(Permission)).Cast<Permission>().ToList();
        foreach (Permission permission in allPermissions.Where(permission => !ownerPermissions.Contains(permission)))
        {
            Assert.Fail($"Owner role is missing permission: {permission}");
        }
    }
}