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
