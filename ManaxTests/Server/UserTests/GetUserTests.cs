using ManaxLibrary.DTO.User;
using ManaxServer.Models.User;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.Server.UserTests;

[TestClass]
public class GetUserTests : UserTestsSetup
{
    [TestMethod]
    public async Task GetUsersReturnsAllUserIds()
    {
        ActionResult<IEnumerable<long>> result = await Controller.GetUsers();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        List<long>? returnedIds = result.Value as List<long>;
        Assert.IsNotNull(returnedIds);
        Assert.AreEqual(Context.Users.Count(), returnedIds.Count);
        foreach (User user in Context.Users) Assert.Contains(user.Id, returnedIds);
    }

    [TestMethod]
    public async Task GetUserWithValidIdReturnsUser()
    {
        User user = Context.Users.First();
        ActionResult<UserDto> result = await Controller.GetUser(user.Id);

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        UserDto? returnedUser = result.Value;
        Assert.IsNotNull(returnedUser);
        Assert.AreEqual(user.Id, returnedUser.Id);
        Assert.AreEqual(user.Username, returnedUser.Username);
        Assert.AreEqual(user.Role, returnedUser.Role);
    }

    [TestMethod]
    public async Task GetUserWithInvalidIdReturnsNotFound()
    {
        ActionResult<UserDto> result = await Controller.GetUser(999999);

        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task GetUserVerifyAllPropertiesMapping()
    {
        User user = Context.Users.First();
        ActionResult<UserDto> result = await Controller.GetUser(user.Id);

        UserDto? returnedUser = result.Value;
        Assert.IsNotNull(returnedUser);
        Assert.AreEqual(user.Id, returnedUser.Id);
        Assert.AreEqual(user.Username, returnedUser.Username);
        Assert.AreEqual(user.Role, returnedUser.Role);
        Assert.AreEqual(user.LastLogin, returnedUser.LastLogin);
    }
}