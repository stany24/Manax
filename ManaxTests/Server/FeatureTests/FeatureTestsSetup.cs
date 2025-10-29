using ManaxServer.Controllers;
using ManaxServer.Services.Feature;
using ManaxTests.Server.Mocks;

namespace ManaxTests.Server.FeatureTests;

public abstract class FeatureTestsSetup
{
    private MockNotificationService _mockNotificationService = null!;
    protected IFeatureService FeatureService = null!;
    protected FeatureController Controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockNotificationService = new MockNotificationService();
        
        FeatureService = new FeatureService(new MockFeatureLoader(),new MockFeatureSaver(),_mockNotificationService);
        Controller = new FeatureController(FeatureService);
    }
}

