using ManaxLibrary.Logging;
using ManaxServer.Localization;

namespace ManaxServer.Services;

public class Service
{
    public Service()
    {
        Logger.LogInfo(Localizer.Format("ServiceInitialized", GetType().Name));
    }
}