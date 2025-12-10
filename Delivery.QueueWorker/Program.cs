using Delivery.Application.Interfaces.Repositories;
using Delivery.Composition;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using Struct.App.Api.Client;

namespace Delivery.QueueWorker
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();

            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var baseUrl = hostContext.Configuration["StructApi:BaseUrl"];
                    var apiKey = hostContext.Configuration["StructApi:ApiKey"];

                    var structApiClient = new StructApiClient(baseUrl, apiKey);

                    var connectionstring = hostContext.Configuration.GetConnectionString("REDIS_URL");
                    services.AddSingleton<IConnectionMultiplexer>(sp =>
                    {
                        if (string.IsNullOrEmpty(connectionstring))
                        {
                            throw new InvalidOperationException("REDIS_URL connection string is missing or empty.");
                        }
                        return ConnectionMultiplexer.Connect(connectionstring);
                    });

                    services.AddSingleton(structApiClient);
                    services.AddApplicationDependencies();
                    services.AddInfrastructureDependencies();
                    services.AddHostedService<QueueWorker>();
                })
                .Build();
                await host.RunAsync();

            //CancellationTokenSource tokenSource = new CancellationTokenSource();
            //while (!tokenSource.Token.IsCancellationRequested)
            //{
            //    // Since we are using the Host above, which requires that the service inherits the backgroundservice class, which inherits the ExecuteAsync() method - maybe we should use that method as the entry point to the queue handling?
            //}
        }
    }
}
