using Delivery.Application.Interfaces.Managers;
using Delivery.Domain.Events;
using Delivery.Domain.Enum;
using System.Text.Json;

namespace Delivery.Application.Services
{
    public class ProductService
    {
        private readonly IQueueManager _queueManager;

        private event EventHandler<ProductUpdatedEventArgs> _productUpdated;

        public ProductService(IQueueManager queueManager)
        {
            _queueManager = queueManager;
        }

        public async Task HandleWebhookAsync(string eventType, JsonElement payload)
        {
            IEnumerable<string> productIds = ExtractProductIds(payload);

            IEnumerable<ProductUpdatedEventArgs> productChanges = productIds.Select(id => new ProductUpdatedEventArgs
            {
                Id = id,
                EventType = eventType,
                EntityType = EntityType.Product
            });

            foreach (var productChange in productChanges)
            {
                _productUpdated?.Invoke(this, productChange);
            }

            await _queueManager.EnqueueUpdatesAsync(productChanges);

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
