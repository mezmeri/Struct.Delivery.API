using Delivery.Application.Interfaces.Repositories;
using Delivery.Composition;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Delivery.QueueWorker
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();
            services.AddInfrastructureDependencies();

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            QueueWorker worker = (QueueWorker) ActivatorUtilities.CreateInstance(serviceProvider, typeof(QueueWorker));

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            while (!tokenSource.Token.IsCancellationRequested)
            {
                //await worker.ProcessQueueAsync(); // QueueWorker should probably also have the token as a parameter in its constructor so that it can react if the token gets cancelled.

                // Specify something to trigger the cancellation of the token below the call.
            }
        }
    }
}
