using ManaxServer.Controllers;
using ManaxServer.Models;
using ManaxServer.Services.Mapper;
using ManaxTests.Server.Mocks;

namespace ManaxTests.Server.SavePointTests;

public abstract class SavePointTestsSetup
{
    private ManaxMapper _mapper = null!;
    protected ManaxContext Context = null!;
    protected SavePointController Controller = null!;
    protected string TestDirectory = null!;

    [TestInitialize]
    public void Setup()
    {
        Context = SqliteTestDbContextFactory.CreateTestContext();
        _mapper = new ManaxMapper(new ManaxMapping());
        Controller = new SavePointController(Context, _mapper);

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