using Delivery.Application.Interfaces.Managers;
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

        public async Task HandleWebhookAsync (JsonElement payload)
        {
            var variantIds = ExtractVariantIds(payload);
            if (!variantIds.Any())
            {
                return;
            }
            await queueManager.EnqueueUpdatesAsync(variantIds);
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
