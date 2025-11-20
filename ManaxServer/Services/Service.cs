using ManaxLibrary.Logging;

namespace ManaxServer.Services;

public abstract class Service
{
    protected Service()
    {
        Logger.LogInfo($"{GetType().Name} initialized.");
    }
}