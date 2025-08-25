using ManaxLibrary.DTO.Issue.Automatic;
using ManaxLibrary.Logging;
using ManaxServer.Middleware;
using ManaxServer.Models;
using ManaxServer.Models.Issue.Reported;
using ManaxServer.Models.Rank;
using ManaxServer.Services.Fix;
using ManaxServer.Services.Hash;
using ManaxServer.Services.Issue;
using ManaxServer.Services.Jwt;
using ManaxServer.Services.Mapper;
using ManaxServer.Services.Notification;
using ManaxServer.Services.Renaming;
using ManaxServer.Services.Task;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using FixService = ManaxServer.Services.Fix.FixService;

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

        AddAuthentication(builder);

        builder.Services.AddDbContext<ManaxContext>(opt =>
            opt.UseSqlite($"Data Source={Path.Combine(AppContext.BaseDirectory, "database.db")}"));

        // Services
        builder.Services.AddSingleton<INotificationService>(provider =>
            new NotificationService(provider.GetRequiredService<IHubContext<NotificationService>>()));
        builder.Services.AddSingleton<IHashService>(_ => new HashService());
        builder.Services.AddSingleton<ITaskService>(provider =>
            new TaskService(provider.GetRequiredService<INotificationService>()));
        builder.Services.AddScoped<IFixService>(provider =>
            new FixService(provider.GetRequiredService<IServiceScopeFactory>(),
                provider.GetRequiredService<IIssueService>()));
        builder.Services.AddScoped<IIssueService>(provider =>
            new IssueService(provider.GetRequiredService<IServiceScopeFactory>()));
        builder.Services.AddScoped<IRenamingService>(provider =>
            new RenamingService(provider.GetRequiredService<IServiceScopeFactory>()));
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

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

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
            IEnumerable<AutomaticIssueSerieType> values =
                Enum.GetValues(typeof(AutomaticIssueSerieType)).Cast<AutomaticIssueSerieType>();
            foreach (AutomaticIssueSerieType serieIssueTypeEnum in values)
                manaxContext.ReportedIssueSerieTypes.Add(new ReportedIssueSerieType
                    { Name = serieIssueTypeEnum.ToString() });
            manaxContext.SaveChanges();
        }

        if (!manaxContext.ReportedIssueChapterTypes.Any())
        {
            IEnumerable<AutomaticIssueChapterType> values =
                Enum.GetValues(typeof(AutomaticIssueChapterType)).Cast<AutomaticIssueChapterType>();
            foreach (AutomaticIssueChapterType serieIssueTypeEnum in values)
                manaxContext.ReportedIssueChapterTypes.Add(new ReportedIssueChapterType
                    { Name = serieIssueTypeEnum.ToString() });
            manaxContext.SaveChanges();
        }
    }

    private static void AddAuthentication(WebApplicationBuilder builder)
    {
        JwtService jwtService = new();
        string secretKey = jwtService.GetSecretKey();
        builder.Services.AddSingleton<IJwtService>(jwtService);

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(secretKey))
                };

                // Special configuration for SignalR
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        StringValues accessToken = context.Request.Query["access_token"];
                        PathString path = context.HttpContext.Request.Path;

                        if (string.IsNullOrEmpty(accessToken) || !path.StartsWithSegments("/notificationHub"))
                            return Task.CompletedTask;
                        context.Token = accessToken;

                        context.Options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(secretKey))
                        };

                        Logger.LogInfo("Token extrait de la requête SignalR: " + path + " - Validation adaptée");

                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        Logger.LogError("Échec d'authentification SignalR", context.Exception, Environment.StackTrace);
                        return Task.CompletedTask;
                    }
                };
            });
    }
}