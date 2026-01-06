using Delivery.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;


internal class Program
{
    static async Task Main(string[] args)
    {
        Console.Title = "Delivery API - Mock Frontend";
        
        // Load configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        var webhookPort = int.Parse(configuration["Webhook:Port"] ?? "5001");

        Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
        Console.WriteLine("║     Starting Delivery API Mock Frontend...           ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
        Console.WriteLine();

        try
        {
            // Connect to Redis
            Console.Write("Connecting to Redis... ");
            var redis = await ConnectionMultiplexer.ConnectAsync(redisConnectionString);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Connected");
            Console.ResetColor();

            var redisMonitor = new RedisMonitor(redis);
            var frontend = new ClientFrontEnd(redisMonitor);

            // Start webhook server in background
            var cts = new CancellationTokenSource();
            _ = Task.Run(() => StartWebhookServer(frontend, webhookPort, cts.Token));

            await Task.Delay(500); // Give server time to start

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Webhook server listening on http://localhost:{webhookPort}");
            Console.WriteLine($"  Endpoint: http://localhost:{webhookPort}/webhook");
            Console.ResetColor();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("ℹ️  This mock frontend will receive notifications when QueueWorker processes items.");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Press any key to open menu...");
            Console.ReadKey();

            // Show interactive menu
            await frontend.ShowMenuAsync();

            cts.Cancel();
            Console.WriteLine("\nShutting down...");
        }
        catch (RedisConnectionException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ Failed to connect to Redis: {ex.Message}");
            Console.WriteLine($"  Connection string: {redisConnectionString}");
            Console.ResetColor();
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ Unexpected error: {ex.Message}");
            Console.ResetColor();
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }

    static async Task StartWebhookServer(ClientFrontEnd frontend, int port, CancellationToken cancellationToken)
    {
        try
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseUrls($"http://localhost:{port}");
            
            // Disable logging noise
            builder.Logging.ClearProviders();

            var app = builder.Build();

            // Webhook endpoint - receives notifications from QueueWorker
            app.MapPost("/webhook", async (HttpContext context) =>
            {
                using var reader = new StreamReader(context.Request.Body);
                var payload = await reader.ReadToEndAsync();
                
                frontend.RecordWebhook(payload);
                
                return Results.Ok(new { status = "received", timestamp = DateTime.UtcNow });
            });

            // Health check endpoint
            app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "mock-frontend" }));

            await app.RunAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Expected when shutting down
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nWebhook server error: {ex.Message}");
            Console.ResetColor();
        }
    }
}
