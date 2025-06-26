using EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

class Program
{
    static void Main(string[] args)
    {
        var connectionString = GetDBConnectionString();

        if (string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("Configuration file should contain a connection string named 'Default'");
            return;
        }

        Console.WriteLine($"Host database: {connectionString}");

        Console.WriteLine("Continue to migration for this host database and all tenants..? (Y/N): ");

        var command = Console.ReadLine();
        if (string.IsNullOrEmpty(command) ||
            command?.ToUpper() != "Y")
        {
            Console.WriteLine("Migration canceled.");
            return;
        }

        Console.WriteLine("HOST database migration started...");

        try
        {
            ApplyMigrations();
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occured during migration of host database:");
            Console.WriteLine(ex.ToString());
            Console.WriteLine("Canceled migrations.");
            return;
        }

        Console.WriteLine("HOST database migration completed.");
        Console.WriteLine("--------------------------------------------------------");
    }

    static void ApplyMigrations()
    {
        using var scope = CreateHost().Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var pendingMigrations = dbContext.Database.GetPendingMigrations();
        Console.WriteLine($"Pending migrations {pendingMigrations.Count()}.");

        dbContext.Database.Migrate();
        Console.WriteLine("Migrations applied successfully.");
    }

    static IHost CreateHost()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var connectionString = GetDBConnectionString();

        return Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(connectionString));
            })
            .Build();
    }

    static string? GetDBConnectionString()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        return configuration?.GetConnectionString("Default");
    }
}