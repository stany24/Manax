using ManaxServer.Controllers;
using ManaxServer.Services.Feature;
using ManaxTests.Server.Mocks;

namespace ManaxTests.Server.FeatureTests;

public abstract class FeatureTestsSetup: TestSetup
{
    private MockNotificationService _mockNotificationService = null!;
    protected FeatureController Controller { get; private set; } = null!;
    protected IFeatureService FeatureService { get; private set; } = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockNotificationService = new MockNotificationService();
        FeatureService = new FeatureService(new MockFeatureLoader(), new MockFeatureSaver(), _mockNotificationService);
        Controller = new FeatureController(FeatureService);
    }
}