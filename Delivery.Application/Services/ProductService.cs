using Delivery.Application.Interfaces.Managers;
using Delivery.Application.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Delivery.Application.Services
{
    public class ProductService
    {
        private readonly IQueueManager _queueManager;

        private readonly ILogger<ProductService> _logger;

        public ProductService(IQueueManager queueManager, ILogger<ProductService> logger)
        {
            _queueManager = queueManager;
            _logger = logger;
        }

        public async Task HandleAttributeWebhookAsync(JsonElement payload)
        {
            var productChanges = ExtractEntityChanges(payload);

            if (!productChanges.Any())
            {
                return;
            }

            await _queueManager.EnqueueEntityUpdatesAsync(productChanges);
        }

        private IEnumerable<EntityItem> ExtractEntityChanges(JsonElement payload)
        {
            var changes = new List<EntityItem>();

            if (payload.TryGetProperty("ProductChanges", out var productChanges))
            {
                //Iterer over hvert item (ProductChanges Prop) i array.
                foreach (var change in productChanges.EnumerateArray())
                {
                    //Kald på ExtractSingleAttributeChange for at få et ProductChangeQueueItem    
                    var queueItem = ExtractSingleEntityChange(change);
                    if (queueItem != null)
                    {
                        changes.Add(queueItem);
                    }
                }
            }

            else if (payload.TryGetProperty("ProductIds", out var ids))
            {            
                foreach(var id in ids.EnumerateArray())
                {
                    var productId = id.ToString();
                    changes.Add(new EntityItem
                    {
                        Id = productId,
                        Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),

                    });

                }
            }
            return changes;
        }
        private EntityItem ExtractSingleEntityChange(JsonElement change)
        {
            if (!change.TryGetProperty("Id", out var id))
            {
                return null;
            }

            var queueItem = new EntityItem
            {
                Id = id.ToString(),
                Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
            };

            //Forsøger prop "ProductModelType" (For later implementation)
            if (change.TryGetProperty("ProductModelType", out var modelType))
            {
                queueItem.EnityModelType = modelType.GetString();
            }

            // Forsøger for UpdatedAttributes
            if (change.TryGetProperty("UpdatedAttributes", out var updatedAttributes))
            {
                // Itererer over hvert item i arrayet
                foreach (var attrName in updatedAttributes.EnumerateArray())
                {
                    //Convert fra JsonVærdi til String
                    string attributeName = attrName.GetString();
                    // Tilføjer til "ChangedAttributes" fra ProductChangeQueueItem
                    queueItem.ChangedAttributes[attributeName] = null;
                }
            }
            // Fallback, checker alt med "Attributes" istedet
            else if (change.TryGetProperty("Attributes", out var attributes))
            {
                ExtractAttributes(queueItem, attributes);
            }

            return queueItem;
        }

        //Tager JSON-object og tilføjer til ChangedAttributes Dictionary i EntityItem
        private void ExtractAttributes(EntityItem queueItem, JsonElement attributes)
        {
            foreach (var attr in attributes.EnumerateObject())
            {
                //Konverter fra JSON til pasende C# type
                var value = DeserializeAttributeValue(attr.Value);
                queueItem.ChangedAttributes[attr.Name] = value;
            }
        }

        //Deserialiserer JasonElement til C# objekt
        private object DeserializeAttributeValue(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.TryGetInt32(out var intVal) ? intVal : (object)element.GetDecimal(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Array => element.EnumerateArray().Select(DeserializeAttributeValue).ToList(),
                JsonValueKind.Object => element.GetRawText(),
                JsonValueKind.Null => null,
                _ => element.GetRawText()
            };
        }
    }
}
