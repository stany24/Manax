using ManaxLibrary.DTOs.Issue.Automatic;
using ManaxLibrary.Logging;
using ManaxServer.Middleware;
using ManaxServer.Models;
using ManaxServer.Models.Issue.Reported;
using ManaxServer.Models.Rank;
using ManaxServer.Services;
using ManaxServer.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;

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

        string secretKey = JwtService.GetSecretKey();
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
                            
                        Logger.LogInfo("Token extrait de la requête SignalR: "+path+" - Validation adaptée");

                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        Logger.LogError("Échec d'authentification SignalR",context.Exception,Environment.StackTrace);
                        return Task.CompletedTask;
                    }
                };
            });

        builder.Services.AddDbContext<ManaxContext>(opt =>
            opt.UseSqlite($"Data Source={Path.Combine(AppContext.BaseDirectory, "database.db")}"));

        // AutoMapper configuration 
        builder.Services.AddAutoMapper(typeof(MappingProfile));

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

        // Initialisation of services
        FixService.Initialize(app.Services.GetRequiredService<IServiceScopeFactory>());
        RenamingService.Initialize(app.Services.GetRequiredService<IServiceScopeFactory>());
        IssueManagerService.Initialize(app.Services.GetRequiredService<IServiceScopeFactory>());
        NotificationService.Initialize(app.Services.GetRequiredService<IHubContext<NotificationHub>>());

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
        
        app.MapHub<NotificationHub>("/notificationHub");

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

        if (!manaxContext.ReportedIssueSerieTypes.Any())
        {
            IEnumerable<AutomaticIssueSerieType> values =
                Enum.GetValues(typeof(AutomaticIssueSerieType)).Cast<AutomaticIssueSerieType>();
            foreach (AutomaticIssueSerieType serieIssueTypeEnum in values)
                manaxContext.ReportedIssueSerieTypes.Add(new ReportedIssueSerieType { Name = serieIssueTypeEnum.ToString() });
            manaxContext.SaveChanges();
        }

        if (!manaxContext.ReportedIssueChapterTypes.Any())
        {
            IEnumerable<AutomaticIssueChapterType> values =
                Enum.GetValues(typeof(AutomaticIssueChapterType)).Cast<AutomaticIssueChapterType>();
            foreach (AutomaticIssueChapterType serieIssueTypeEnum in values)
                manaxContext.ReportedIssueChapterTypes.Add(new ReportedIssueChapterType { Name = serieIssueTypeEnum.ToString() });
            manaxContext.SaveChanges();
        }
    }
}