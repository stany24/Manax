using ManaxApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ManaxApi.Models;
using ManaxApi.Models.Issue;
using ManaxApi.Middleware;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace ManaxApi;

public static class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHttpContextAccessor();
        
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
            });

        builder.Services.AddDbContext<ManaxContext>(opt =>
            opt.UseSqlite($"Data Source={Path.Combine(AppContext.BaseDirectory, "database.db")}"));
        
        // Configuration AutoMapper
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
        
        // Initialisation du singleton ScanService
        ScanService.Initialize(app.Services.GetRequiredService<IServiceScopeFactory>());
        IssueManagerService.Initialize(app.Services.GetRequiredService<IServiceScopeFactory>());
        
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
                new Models.Rank.Rank { Name = "SSS", Value = 16 },
                new Models.Rank.Rank { Name = "SS", Value = 14 },
                new Models.Rank.Rank { Name = "S", Value = 12 },
                new Models.Rank.Rank { Name = "A", Value = 10 },
                new Models.Rank.Rank { Name = "B", Value = 8 },
                new Models.Rank.Rank { Name = "C", Value = 6 },
                new Models.Rank.Rank { Name = "D", Value = 4 },
                new Models.Rank.Rank { Name = "E", Value = 2 }
            );
            manaxContext.SaveChanges();
        }

        if (!manaxContext.SerieIssueTypes.Any())
        {
            IEnumerable<SerieIssueTypeEnum> values = Enum.GetValues(typeof(SerieIssueTypeEnum)).Cast<SerieIssueTypeEnum>();
            foreach (SerieIssueTypeEnum serieIssueTypeEnum in values)
            {
                manaxContext.SerieIssueTypes.Add(new SerieIssueType {Name = serieIssueTypeEnum.ToString()});
            }
            manaxContext.SaveChanges();
        }
        
        if (!manaxContext.ChapterIssueTypes.Any())
        {
            IEnumerable<ChapterIssueTypeEnum> values = Enum.GetValues(typeof(ChapterIssueTypeEnum)).Cast<ChapterIssueTypeEnum>();
            foreach (ChapterIssueTypeEnum serieIssueTypeEnum in values)
            {
                manaxContext.ChapterIssueTypes.Add(new ChapterIssueType {Name = serieIssueTypeEnum.ToString()});
            }
            manaxContext.SaveChanges();
        }
    }
}