using Delivery.Application.Interfaces.Managers;
using System.Text.Json;

namespace Delivery.Application.Services
{
    public class ProductService
    {
        private readonly IQueueManager _queueManager;

        public ProductService(IQueueManager queueManager)
        {
            _queueManager = queueManager;
        }

        public async Task HandleWebhookAsync(JsonElement payload)
        {
            var productIds = ExtractProductIds(payload);
            if (!productIds.Any())
            {
                return;
            }

            await _queueManager.EnqueueUpdatesAsync(productIds);

        }

        private IEnumerable<string> ExtractProductIds(JsonElement payload)
        {
            if (payload.TryGetProperty("ProductIds", out var ids))
            {
                return ids.EnumerateArray().Select(id => id.ToString());
            }

            if (payload.TryGetProperty("ProductChanges", out var productChanges))
            {
                return productChanges.EnumerateArray()
                    .Select(c => c.TryGetProperty("Id", out var id) ? id.ToString() : null)
                    .OfType<string>(); 
            }

            return Enumerable.Empty<string>();
        }
    }
}
