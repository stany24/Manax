using ManaxApi.Models;
using ManaxApi.Models.User;
using Microsoft.EntityFrameworkCore;

namespace ManaxApi;

public static class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        builder.Services.AddDbContext<LibraryContext>(opt =>
            opt.UseSqlite("Data Source=database.db"));
        builder.Services.AddDbContext<UserContext>(opt =>
            opt.UseSqlite("Data Source=database.db"));

        WebApplication app = builder.Build();

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

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}