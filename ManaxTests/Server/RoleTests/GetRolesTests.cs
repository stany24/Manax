using ManaxLibrary.DTO.Role;
using ManaxServer.Models.Person;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.Server.RoleTests;

[TestClass]
public class GetRolesTests : RoleTestsSetup
{
    [TestMethod]
    public async Task GetRolesReturnsAllRoles()
    {
        Role role1 = new() { Name = "Author" };
        Role role2 = new() { Name = "Artist" };
        Context.Roles.AddRange(role1, role2);
        await Context.SaveChangesAsync();

        ActionResult<IEnumerable<RoleDto>> result = await Controller.GetRoles();
        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        IEnumerable<RoleDto>? value = okResult.Value as IEnumerable<RoleDto>;
        Assert.IsNotNull(value);
        List<RoleDto> roles = value.ToList();
        Assert.HasCount(2, roles);
        Assert.IsTrue(roles.Any(r => r.Name == "Author"));
        Assert.IsTrue(roles.Any(r => r.Name == "Artist"));
    }

    [TestMethod]
    public async Task GetRolesReturnsEmptyListWhenNoRoles()
    {
        ActionResult<IEnumerable<RoleDto>> result = await Controller.GetRoles();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);

        IEnumerable<RoleDto>? value = okResult.Value as IEnumerable<RoleDto>;
        Assert.IsNotNull(value);
        List<RoleDto> roles = value.ToList();
        Assert.IsEmpty(roles);
    }

    [TestMethod]
    public async Task GetRolesReturnsRolesWithCorrectProperties()
    {
        Role role = new() { Name = "Editor" };
        Context.Roles.Add(role);
        await Context.SaveChangesAsync();

        ActionResult<IEnumerable<RoleDto>> result = await Controller.GetRoles();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);

        IEnumerable<RoleDto>? value = okResult.Value as IEnumerable<RoleDto>;
        Assert.IsNotNull(value);
        List<RoleDto> roles = value.ToList();
        RoleDto returnedRole = roles.First();
        Assert.AreEqual("Editor", returnedRole.Name);
        Assert.IsGreaterThan(0, returnedRole.Id);
    }
}