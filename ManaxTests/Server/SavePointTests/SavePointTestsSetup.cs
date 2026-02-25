using ManaxServer.Controllers;
using ManaxServer.Models;

namespace ManaxTests.Server.SavePointTests;

public abstract class SavePointTestsSetup
{
    protected ManaxContext Context { get; private set; } = null!;
    protected SavePointController Controller { get; private set; } = null!;
    protected string TestDirectory { get; private set; } = null!;

    [TestInitialize]
    public void Setup()
    {
        Context = SqliteTestDbContextFactory.CreateTestContext();
        Controller = new SavePointController(Context);

        TestDirectory = Path.Combine(Path.GetTempPath(), "SavePointTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(TestDirectory);
    }

    [TestCleanup]
    public void Cleanup()
    {
        SqliteTestDbContextFactory.CleanupTestDatabase(Context);

        if (!Directory.Exists(TestDirectory)) return;
        try
        {
            Directory.Delete(TestDirectory, true);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}