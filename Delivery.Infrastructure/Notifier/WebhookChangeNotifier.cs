using Delivery.Application.Interfaces.Notifier;
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
        HttpClient _httpClient;
        public WebhookChangeNotifier(HttpClient httpClient) 
        {
            _httpClient = httpClient;
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

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            await _httpClient.PostAsync("", content);
        }
    }
}
