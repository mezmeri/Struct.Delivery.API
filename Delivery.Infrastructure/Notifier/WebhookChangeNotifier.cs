using Delivery.Application.Interfaces.Notifier;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Delivery.Infrastructure.Notifier
{
    public class WebhookChangeNotifier : INotifier
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WebhookChangeNotifier> _logger;
        private readonly string _webhookUrl;

        public WebhookChangeNotifier(HttpClient httpClient, ILogger<WebhookChangeNotifier> logger, string webhookUrl = "http://localhost:5001/webhook") 
        {
            _httpClient = httpClient;
            _logger = logger;
            _webhookUrl = webhookUrl;
        }

        public async Task NotifyChangesAsync(IEnumerable<string> ids, string eventType, DateTimeOffset timestamp, string entityType)
        {
            var payload = new
            {
                Ids = ids,
                EventType = eventType,
                Timestamp = timestamp,
                EntityType = entityType
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(_webhookUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Succesfully notificeret, sendt til {_webhookUrl}. EventType: {EventType}, EntityType: {EntityType}, Ids: {ids.Count()}", _webhookUrl, eventType, entityType, string.Join(", ", ids));
                }
                else
                {
                    _logger.LogError("Failed to notify changes to webhook. StatusCode: {StatusCode}, EventType: {EventType}, EntityType: {EntityType}, Ids: {Ids}", response.StatusCode, eventType, entityType, string.Join(", ", ids));
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning($"Failed to send webhook notification to {_webhookUrl}: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error sending webhook notification to {_webhookUrl}");
            }
        }
    }
}
