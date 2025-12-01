using System.Net;
using System.Threading.RateLimiting;
using ManaxServer.Middleware;
using ManaxServer.Models;
using ManaxServer.Models.Issue.Reported;
using ManaxServer.Models.Person;
using ManaxServer.Models.Rank;
using ManaxServer.Services.BackgroundTask;
using ManaxServer.Services.Feature;
using ManaxServer.Services.Fix;
using ManaxServer.Services.Hash;
using ManaxServer.Services.Issue;
using ManaxServer.Services.Notification;
using ManaxServer.Services.Permission;
using ManaxServer.Services.Token;
using ManaxServer.Services.Validation;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer;

public static class Program

{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("StrictNoCrossOrigin", policy =>
            {
                policy.SetIsOriginAllowed(_ => false);
                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
            });
        });

        // SignalR configuration
        builder.Services.AddSignalR();

        builder.Services.AddDbContext<ManaxContext>(opt =>
            opt.UseSqlite($"Data Source={Path.Combine(AppContext.BaseDirectory, "database.db")}"));

        builder.Services.AddSingleton<IHashService, HashService>();
        builder.Services.AddSingleton<IPermissionService, PermissionService>();
        builder.Services.AddSingleton<ITokenService, TokenService>();
        builder.Services.AddSingleton<IPasswordValidationService>(_ => 
            new PasswordValidationService(builder.Environment.IsProduction()));
        
        builder.Services.AddSingleton<INotificationService, NotificationService>();
        builder.Services.AddSingleton<IBackgroundTaskService, BackgroundTaskService>();
        builder.Services.AddSingleton<IIssueService, IssueService>();
        builder.Services.AddSingleton<IFixService, FixService>();
        
        FeatureFileManager featureFileManager = new();
        builder.Services.AddSingleton<IFeatureLoader>(featureFileManager);
        builder.Services.AddSingleton<IFeatureSaver>(featureFileManager);
        builder.Services.AddSingleton<IFeatureService, FeatureService>();
        
        AddAuthentication(builder);
        AddRateLimiting(builder);

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

        app.UseCors("StrictNoCrossOrigin");

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
                new IssueChapterReportedType { Name = "Missing pages" },
                new IssueChapterReportedType { Name = "Wrong order" },
                new IssueChapterReportedType { Name = "Bad quality" });
            manaxContext.SaveChanges();
        }

        if (!manaxContext.ReportedIssueSerieTypes.Any())
        {
            manaxContext.ReportedIssueSerieTypes.AddRange(
                new IssueSerieReportedType { Name = "Wrong description" },
                new IssueSerieReportedType { Name = "Wrong poster" },
                new IssueSerieReportedType { Name = "Wrong name" });
            manaxContext.SaveChanges();
        }

        if (!manaxContext.Roles.Any())
        {
            manaxContext.Roles.AddRange(
                new Role { Name = "Author" },
                new Role { Name = "Writer" },
                new Role { Name = "Artist" });
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

        builder.Services.AddAuthentication()
            .AddBearerToken(options => { options.BearerTokenExpiration = TimeSpan.FromHours(12); });
    }
}