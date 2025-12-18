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
    public class ProductStructureService
    {
        private readonly IQueueManager _queueManager;

        private readonly ILogger<ProductStructureService> _logger;

        public ProductStructureService(IQueueManager queueManager, ILogger<ProductStructureService> logger)
        {
            _queueManager = queueManager;
            _logger = logger;
        }

        public async Task HandleWebhookAsync(string eventType, JsonElement payload)
        {
            IEnumerable<string> productStructureIds = ExtractProductStructureIds(payload);

            IEnumerable<ProductStructureUpdatedDTO> productStructureChanges = productStructureIds.Select(id => new ProductStructureUpdatedDTO
            {
                Id = id,
                EventType = eventType,
                EntityType = EntityType.ProductStructure
            });

            await _queueManager.EnqueueUpdatesAsync(productStructureChanges);

        }

        private IEnumerable<string> ExtractProductStructureIds(JsonElement payload)
        {
            if (!payload.TryGetProperty("ProductStructureUid", out var ids))
            {
                return ids.EnumerateArray().Select(id => id.ToString());
            }

            if (ids.ValueKind == JsonValueKind.String)
            {
                return [ids.GetString()];
            }

            return Enumerable.Empty<string>();
        }
    }
}
