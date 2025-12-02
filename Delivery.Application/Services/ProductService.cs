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

        public async Task HandleWebhookAsync(JsonElement payload)
        {
            var productChanges = ExtractProductChanges(payload);

            if (!productChanges.Any())
            {
                return;
            }

            await _queueManager.EnqueueUpdatesAsync(productChanges);
        }

        private IEnumerable<ProductChangeQueueItem> ExtractProductChanges(JsonElement payload)
        {
            var changes = new List<ProductChangeQueueItem>();

            if (payload.TryGetProperty("ProductChanges", out var productChanges))
            {
                //Iterer over hvert item (ProductChanges Prop) i array.
                foreach (var change in productChanges.EnumerateArray())
                {
                    //Kald på ExtractSingleProductChange for at få et ProductChangeQueueItem    
                    var queueItem = ExtractSingleProductChange(change);
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
                    changes.Add(new ProductChangeQueueItem
                    {
                        ProductId = productId,
                        Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),

                    });

                }
            }
            return changes;
        }
        private ProductChangeQueueItem ExtractSingleProductChange(JsonElement change)
        {
            if (!change.TryGetProperty("Id", out var id))
            {
                return null;
            }

            var queueItem = new ProductChangeQueueItem
            {
                ProductId = id.ToString(),
                Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
            };

            //Forsøger prop "ProductModelType" (For later implementation)
            if (change.TryGetProperty("ProductModelType", out var modelType))
            {
                queueItem.ProductModelType = modelType.GetString();
            }

            // Forsøger for UpdatedAttributes (Struct PIM format)
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
            // Fallback, checker "Attributes" istedet
            else if (change.TryGetProperty("Attributes", out var attributes))
            {
                ExtractAttributes(queueItem, attributes);
            }

            return queueItem;
        }

        //Tager JSON-object og tilføjer til ChangedAttributes Dictionary
        // for ProductChangeQueueItem
        private void ExtractAttributes(ProductChangeQueueItem queueItem, JsonElement attributes)
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
                JsonValueKind.Object => element.GetRawText(), // Store complex objects as JSON string
                JsonValueKind.Null => null,
                _ => element.GetRawText()
            };
        }
    }
}
