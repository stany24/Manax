using ManaxLibrary.Logging;
using ManaxServer.Localization;

namespace ManaxServer.Services;

public abstract class Service
{
    protected Service()
    {
        Logger.LogInfo(Localizer.Format("ServiceInitialized", GetType().Name));
    }
}