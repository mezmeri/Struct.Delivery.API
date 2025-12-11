using Delivery.Application.Interfaces.Managers;
using Delivery.Application.Interfaces.Notifier;
using Delivery.Application.Interfaces.Repositories;
using Delivery.Infrastructure.Managers;
using Delivery.Infrastructure.Notifier;
using Delivery.Infrastructure.Persistence.Redis.Read;
using Delivery.Infrastructure.Persistence.Redis.Write;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace Delivery.Infrastructure
{
    public static class DependencyRegistration
    {
        public static IServiceCollection ConfigureWriteRepositories(this IServiceCollection services)
        {
            services.AddSingleton<IProductWriteRepository, ProductWriteRepository>();
            services.AddSingleton<IVariantWriteRepository, VariantWriteRepository>();
            services.AddSingleton<IVariantGroupWriteRepository, VariantGroupWriteRepository>();
            services.AddSingleton<ICatalogueWriteRepository, CatalogueWriteRepository>();
            services.AddSingleton<ICategoryWriteRepository, CategoryWriteRepository>();
            services.AddSingleton<IAttributeWriteRepository, AttributeWriteRepository>();
            services.AddSingleton<IAttributeScopeWriteRepository, AttributeScopeWriteRepository>();
            services.AddSingleton<IQueueWriteRepository, QueueWriteRepository>();

            return services;
        }

        public static IServiceCollection ConfigureReadRepositories(this IServiceCollection services)
        {
            services.AddSingleton<IProductReadRepository, ProductReadRepository>();
            services.AddSingleton<IQueueReadRepository, QueueReadRepository>();

            return services;
        }

        public static IServiceCollection ConfigureNotifiers(this IServiceCollection services, IConfiguration configuration)
        {
            //Konfigurerbart webhook URL, ellers fallback til Localhost for udvikling
            var webhookUrl = configuration["Webhook:Url"] ?? "http://localhost:5001/webhook";

            // Register as singleton with factory pattern
            services.AddSingleton<INotifier>(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient(nameof(WebhookChangeNotifier));
                var logger = sp.GetRequiredService<ILogger<WebhookChangeNotifier>>();
                return new WebhookChangeNotifier(httpClient, logger, webhookUrl);
            });

            // Register named HttpClient for the notifier
            services.AddHttpClient(nameof(WebhookChangeNotifier))
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true // For development only
                });

            return services;
        }
    }
}
