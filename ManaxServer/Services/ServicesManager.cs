using Microsoft.AspNetCore.SignalR;

namespace ManaxServer.Services;

public static class ServicesManager
{
    public static HashService Hash { get; private set; }
    public static JwtService Jwt { get; private set; }
    public static TaskService Task { get; private set; }
    
    public static FixService Fix { get; private set; } = null!;
    public static IssueService Issue { get; private set; } = null!;
    public static NotificationService Notification { get; private set; } = null!;
    public static RenamingService Renaming { get; private set; } = null!;

    static ServicesManager()
    {
        Hash = new HashService();
        Jwt = new JwtService();
        Task = new TaskService();
    }
    
    public static void Initialize(WebApplication app)
    {
        Fix = new FixService(app.Services.GetRequiredService<IServiceScopeFactory>());
        Issue = new IssueService(app.Services.GetRequiredService<IServiceScopeFactory>());
        Notification = new NotificationService(app.Services.GetRequiredService<IHubContext<NotificationService>>());
        Renaming = new RenamingService(app.Services.GetRequiredService<IServiceScopeFactory>());
    }
}