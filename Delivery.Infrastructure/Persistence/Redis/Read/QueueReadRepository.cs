using Delivery.Application.Interfaces.Repositories;
using Delivery.Application.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Struct.App.Api.Models.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Delivery.Infrastructure.Persistence.Redis.Read
{
    public class QueueReadRepository : IQueueReadRepository
    {
        private readonly IDatabase _database;
        private readonly ILogger<QueueReadRepository> _logger;
        private readonly string ProductUpdateQueueName = "products:updates:pending";
        private readonly string ProductTimestamps = "products:updates:timestamps";
        private readonly string ProductQueueList = "products:updates:list";
        private readonly string ProductAttributeChanges = "products:updates:attributes";

        public QueueReadRepository(IConnectionMultiplexer redis, ILogger<QueueReadRepository> logger)
        {
            _database = redis.GetDatabase();
            _logger = logger;
        }

        public async Task<IEnumerable<ProductChangeQueueItem>> GetProductChanges(int batchSize = 100)
        {

            var result = new List<ProductChangeQueueItem>();

            for (int i = 0; i < batchSize; i++)
            {
                RedisValue item = await _database.ListLeftPopAsync(ProductQueueList);

                if (!item.HasValue)
                {
                    break;
                }

                string productId = item.ToString();
                long timestamp = (long)(await _database.SortedSetScoreAsync(ProductTimestamps, item)).GetValueOrDefault();

                var queueItem = new ProductChangeQueueItem
                {
                    ProductId = productId,
                    Timestamp = timestamp
                };

                // Finder ændrede attributter fra Redis
                // Might need some work, er i tvivl om placering af logik. 
                // Got help from CoPilot, så lad os lige vende tilbage til denne del.
                string attributesJson = await _database.StringGetAsync($"{ProductAttributeChanges}:{productId}");

                if (!string.IsNullOrEmpty(attributesJson))
                {
                    try
                    {
                        queueItem.ChangedAttributes = JsonSerializer.Deserialize<Dictionary<string, object>>(attributesJson);

                        _logger.LogDebug("Product {ProductId} has {Count} attribute changes: {Attributes}",
                            productId,
                            queueItem.ChangedAttributes?.Count ?? 0,
                            queueItem.ChangedAttributes != null ? string.Join(", ", queueItem.ChangedAttributes.Keys) : "none");
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Failed to deserialize attribute changes for product {ProductId}. JSON: {Json}",
                            productId, attributesJson);
                    }
                }
                else
                {
                    _logger.LogDebug("Product {ProductId} has no attribute changes stored.", productId);
                }

                result.Add(queueItem);
            }

            _logger.LogInformation("Retrieved {Count} products from Redis queue.", result.Count);

            return result;
        }
    }
}
