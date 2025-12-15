using Delivery.Application.Interfaces.Notifier;
using Delivery.Infrastructure.Notifier;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Infrastructure
{
    public static class ClientDependencyRegistration
    {
        public static IServiceCollection ConfigureNotifiers(this IServiceCollection services, IConfiguration configuration)
        {
            // Konfigurerbart webhook URL, ellers fallback til Localhost for udvikling
            var webhookUrl = configuration["Webhook:Url"] ?? "http://localhost:5001/webhook";

            // Register as singleton with factory pattern - Needed for at create HttpClient
            services.AddSingleton<INotifier>(sp =>
            {
                // Beder vores DI container om IHttpClientFactory
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                // Opretter en HttpClient specifikt til WebhookChangeNotifier
                var httpClient = httpClientFactory.CreateClient(nameof(WebhookChangeNotifier));
                var logger = sp.GetRequiredService<ILogger<WebhookChangeNotifier>>();

                // Returnerer en ny instans af WebhookChangeNotifier med required params
                return new WebhookChangeNotifier(httpClient, logger, webhookUrl);
            });

            // Registrerer named HttpClient for WebhookChangeNotifier
            services.AddHttpClient(nameof(WebhookChangeNotifier))
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true // For development only
                });

            return services;
        }
    }
}
