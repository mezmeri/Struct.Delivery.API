using Delivery.Application.Interfaces.Managers;
using Delivery.Domain.DTO;
using Delivery.Domain.Enum;
using Delivery.Domain.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Delivery.Application.Services
{
    public class VariantGroupService
    {
        private readonly IQueueManager _queueManager;

        private readonly ILogger<VariantGroupService> _logger;

        public VariantGroupService(IQueueManager queueManager, ILogger<VariantGroupService> logger)
        {
            _queueManager = queueManager;
            _logger = logger;
        }

        public async Task HandleWebhookAsync(string eventType, JsonElement payload)
        {
            IEnumerable<string> variantGroupIds = ExtractVariantGroupIds(payload);

            IEnumerable<VariantGroupUpdatedDTO> variantGroupChanges = variantGroupIds.Select(id => new VariantGroupUpdatedDTO
            {
                Id = id,
                EventType = eventType,
                EntityType = EntityType.VariantGroup
            });

            await _queueManager.EnqueueUpdatesAsync(variantGroupChanges);

        }

        private IEnumerable<string> ExtractVariantGroupIds(JsonElement payload)
        {
            if (payload.TryGetProperty("VariantGroupIds", out var ids))
            {
                return ids.EnumerateArray().Select(id => id.ToString());
            }

            if (payload.TryGetProperty("VariantGroupsChanges", out var variantGroupChanges))
            {
                return variantGroupChanges.EnumerateArray()
                    .Select(c => c.TryGetProperty("Id", out var id) ? id.ToString() : null)
                    .OfType<string>();
            }

            return Enumerable.Empty<string>();
        }
    }
}
