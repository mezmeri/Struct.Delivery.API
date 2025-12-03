using Delivery.Application.Interfaces.Managers;
using Delivery.Domain.Events;
using Delivery.Domain.Enum;
using System.Text.Json;

namespace Delivery.Application.Services
{
    public class VariantService
    {
        private readonly IQueueManager queueManager;

        public VariantService(IQueueManager queueManager)
        {
            this.queueManager = queueManager;
        }

        public async Task HandleWebhookAsync (JsonElement payload, string eventType)
        {
            IEnumerable<string> variantIds = ExtractVariantIds(payload);

            IEnumerable<VariantUpdated> variantChanges = variantIds.Select(id => new VariantUpdated
            {
                Id = id,
                EventType = eventType,
                EntityType = EntityType.Variant
            });
            await queueManager.EnqueueUpdatesAsync(variantChanges);
        }

        private IEnumerable<string> ExtractVariantIds(JsonElement payload)
        {
            if (payload.TryGetProperty("VariantIds", out var ids))
            {
                return ids.EnumerateArray().Select(id => id.ToString());
            }

            if (payload.TryGetProperty("VariantChanges", out var variantChanges))
            {
                return variantChanges.EnumerateArray()
                    .Select(c => c.TryGetProperty("Id", out var id) ? id.ToString() : null)
                    .OfType<string>();
            }

            return Enumerable.Empty<string>();
        }
    }
}
