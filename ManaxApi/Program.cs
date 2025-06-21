using ManaxApi.Models.Chapter;
using ManaxApi.Models.Issue;
using ManaxApi.Models.Library;
using ManaxApi.Models.Read;
using ManaxApi.Models.Serie;
using ManaxApi.Models.User;
using ManaxApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ManaxApi;

public static class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Configuration de l'authentification JWT avec la clé obtenue de JwtService
        // La méthode GetSecretKey va générer une clé si elle n'existe pas
        string secretKey = JwtService.GetSecretKey(builder.Configuration);
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(secretKey))
                };
            });

        builder.Services.AddDbContext<UserContext>(opt =>
            opt.UseSqlite($"Data Source={Path.Combine(AppContext.BaseDirectory, "database.db")}"));

        builder.Services.AddDbContext<LibraryContext>(opt =>
            opt.UseSqlite($"Data Source={Path.Combine(AppContext.BaseDirectory, "database.db")}"));
        builder.Services.AddDbContext<SerieContext>(opt =>
            opt.UseSqlite($"Data Source={Path.Combine(AppContext.BaseDirectory, "database.db")}"));
        builder.Services.AddDbContext<ChapterContext>(opt =>
            opt.UseSqlite($"Data Source={Path.Combine(AppContext.BaseDirectory, "database.db")}"));

        builder.Services.AddDbContext<IssueContext>(opt =>
            opt.UseSqlite($"Data Source={Path.Combine(AppContext.BaseDirectory, "database.db")}"));
        builder.Services.AddDbContext<ReadContext>(opt =>
            opt.UseSqlite($"Data Source={Path.Combine(AppContext.BaseDirectory, "database.db")}"));

        WebApplication app = builder.Build();

        // Création/mise à jour automatique de la base de données
        using (IServiceScope scope = app.Services.CreateScope())
        {
            DbContext[] dbContexts =
            [
                scope.ServiceProvider.GetRequiredService<LibraryContext>(),
                scope.ServiceProvider.GetRequiredService<UserContext>()
            ];
            foreach (DbContext db in dbContexts) db.Database.Migrate();
        }

        // Middleware global de gestion des exceptions
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\":\"Internal server error\"}");
            });
        });

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        // Ajouter le middleware d'authentification avant celui d'autorisation
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}