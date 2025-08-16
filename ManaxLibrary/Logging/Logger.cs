namespace ManaxLibrary.Logging;

public static class Logger
{
    private static readonly string Path;
    private static readonly object FileLock = new();
    private static readonly object ConsoleLock = new();

    static Logger()
    {
        Path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs.txt");
    }

    public static void LogInfo(string message)
    {
        string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{LogType.Info}] {message}";
        LogToFile(logMessage);
        LogToConsole(logMessage, LogType.Info);
    }

    public static void LogWarning(string message, string stackTrace)
    {
        LogProblem(message, stackTrace, LogType.Warning);
    }

    public static void LogFailure(string message, string stackTrace)
    {
        LogProblem(message, stackTrace, LogType.Failure);
    }

    public static void LogError(string message, Exception e, string stackTrace)
    {
        string logMessage =
            $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{LogType.Error}] {message} error: {e} at: {stackTrace}";
        LogToFile(logMessage);
        LogToConsole(logMessage, LogType.Error);
    }

    private static void LogProblem(string message, string stackTrace, LogType type)
    {
        string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{type}] {message} at: {stackTrace}";
        LogToFile(logMessage);
        LogToConsole(logMessage, type);
    }

    private static void LogToFile(string message)
    {
        lock (FileLock)
        {
            File.AppendAllText(Path, message + Environment.NewLine);
        }
    }

    private static void LogToConsole(string message, LogType type)
    {
        lock (ConsoleLock)
        {
            Console.ForegroundColor = type switch
            {
                LogType.Info => ConsoleColor.White,
                LogType.Warning => ConsoleColor.Yellow,
                LogType.Failure => ConsoleColor.DarkYellow,
                LogType.Error => ConsoleColor.Red,
                _ => ConsoleColor.White
            };

            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}