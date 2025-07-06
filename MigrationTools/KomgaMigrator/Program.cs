using ManaxApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KomgaMigrationTool;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: MigrationProgram <source-db-path>");
            return;
        }

        string sourceDbPath = args[0];
        if (!File.Exists(sourceDbPath))
        {
            Console.WriteLine($"Source database file not found: {sourceDbPath}");
            return;
        }

        // Setup DI
        ServiceProvider serviceProvider = new ServiceCollection()
            .AddLogging(builder => 
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            })
            .AddDbContext<ManaxContext>(options => 
            {
                // Use your connection string here
                options.UseSqlite("Data Source=database.db");
            })
            .BuildServiceProvider();

        ILogger<MigrationTool> logger = serviceProvider.GetService<ILoggerFactory>()
            .CreateLogger<MigrationTool>();

        // Run the migration
        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            ManaxContext? dbContext = scope.ServiceProvider.GetService<ManaxContext>();
                
            // Make sure the database is created and all migrations are applied
            dbContext.Database.Migrate();

            // Create and run the migration tool
            MigrationTool migrationTool = new(sourceDbPath, dbContext, logger);
            migrationTool.MigrateData();
        }

        Console.WriteLine("Migration completed successfully!");
    }
}