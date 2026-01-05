using ManaxLibrary;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.Server;

public class TestSetup
{
    internal static void CheckTypeAndErrorCode<T>(ActionResult? result,ErrorCode code) where T : ObjectResult
    {
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<T>(result);
        T notFound = (T)result;
        Assert.AreEqual(code, notFound.Value);
    }
}