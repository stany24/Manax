using System.Net;
using System.Threading.RateLimiting;
using ManaxServer.Controllers;
using ManaxServer.Localization;
using ManaxServer.Middleware;
using ManaxServer.Models;
using ManaxServer.Models.Issue.Reported;
using ManaxServer.Models.Rank;
using ManaxServer.Services.BackgroundTask;
using ManaxServer.Services.Fix;
using ManaxServer.Services.Hash;
using ManaxServer.Services.Issue;
using ManaxServer.Services.Mapper;
using ManaxServer.Services.Notification;
using ManaxServer.Services.Permission;
using ManaxServer.Services.Renaming;
using ManaxServer.Services.Token;
using ManaxServer.Services.Validation;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHttpContextAccessor();

        // SignalR configuration
        builder.Services.AddSignalR();

        builder.Services.AddDbContext<ManaxContext>(opt =>
            opt.UseSqlite($"Data Source={Path.Combine(AppContext.BaseDirectory, "database.db")}"));

        AddAuthentication(builder);

        // Services
        builder.Services.AddSingleton<INotificationService>(provider =>
            new NotificationService(
                provider.GetRequiredService<IHubContext<NotificationService>>(),
                provider.GetRequiredService<IPermissionService>()));
        builder.Services.AddSingleton<IHashService>(_ => new HashService());
        builder.Services.AddSingleton<IRenamingService>(provider =>
            new RenamingService(provider.GetRequiredService<IServiceScopeFactory>()));
        builder.Services.AddSingleton<IBackgroundTaskService>(provider =>
            new BackgroundTaskService(provider.GetRequiredService<INotificationService>()));
        builder.Services.AddSingleton<IIssueService>(provider =>
            new IssueService(provider.GetRequiredService<IServiceScopeFactory>()));
        builder.Services.AddSingleton<IFixService>(provider =>
            new FixService(provider.GetRequiredService<IServiceScopeFactory>(),
                provider.GetRequiredService<IIssueService>()));
        builder.Services.AddSingleton<IPasswordValidationService>(_ =>
            new PasswordValidationService(builder.Environment.IsProduction()));
        AddRateLimiting(builder);

        builder.Services.AddScoped<IMapper>(_ => new ManaxMapper(new ManaxMapping()));

        builder.Services.Configure<KestrelServerOptions>(options =>
        {
            options.Limits.MaxRequestBodySize = int.MaxValue;
            options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(3);
            options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(20);
        });

        builder.Services.Configure<FormOptions>(options =>
        {
            options.ValueLengthLimit = int.MaxValue;
            options.MultipartBodyLengthLimit = int.MaxValue;
            options.MemoryBufferThreshold = int.MaxValue;
        });

        WebApplication app = builder.Build();

        Migrate(app);

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseRateLimiter();
        app.UseMiddleware<BearerAuthenticationMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.MapHub<NotificationService>("/notificationHub");

        app.Run();
    }

    private static void Migrate(WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        ManaxContext manaxContext = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        manaxContext.Database.Migrate();

        if (!manaxContext.Ranks.Any())
        {
            manaxContext.Ranks.AddRange(
                new Rank { Name = "SSS", Value = 16 },
                new Rank { Name = "SS", Value = 14 },
                new Rank { Name = "S", Value = 12 },
                new Rank { Name = "A", Value = 10 },
                new Rank { Name = "B", Value = 8 },
                new Rank { Name = "C", Value = 6 },
                new Rank { Name = "D", Value = 4 },
                new Rank { Name = "E", Value = 2 }
            );
            manaxContext.SaveChanges();
        }

        if (!manaxContext.ReportedIssueChapterTypes.Any())
        {
            manaxContext.ReportedIssueChapterTypes.AddRange(
                new ReportedIssueChapterType { Name = "Missing pages" },
                new ReportedIssueChapterType { Name = "Wrong order" },
                new ReportedIssueChapterType { Name = "Bad quality" });
            manaxContext.SaveChanges();
        }

        if (!manaxContext.ReportedIssueSerieTypes.Any())
        {
            manaxContext.ReportedIssueSerieTypes.AddRange(
                new ReportedIssueSerieType { Name = "Wrong description" },
                new ReportedIssueSerieType { Name = "Wrong poster" },
                new ReportedIssueSerieType { Name = "Wrong name" });
            manaxContext.SaveChanges();
        }
    }

    private static void AddRateLimiting(WebApplicationBuilder builder)
    {
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = (int)HttpStatusCode.TooManyRequests;
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", token);
            };
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                string clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 300,
                    QueueLimit = 100,
                    Window = TimeSpan.FromMinutes(1)
                });
            });
        });
    }

    private static void AddAuthentication(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IPermissionService>(provider =>
            new PermissionService(provider.GetRequiredService<IServiceScopeFactory>()));

        IPermissionService permissionService =
            builder.Services.BuildServiceProvider().GetRequiredService<IPermissionService>();
        TokenService tokenService = new(permissionService);
        builder.Services.AddSingleton<ITokenService>(tokenService);

        builder.Services.AddAuthentication()
            .AddBearerToken(options => { options.BearerTokenExpiration = TimeSpan.FromHours(12); });
    }
}