using Delivery.Application.Interfaces.Repositories;
using Delivery.Application.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Delivery.Infrastructure.Persistence.Redis.Write
{
    public class QueueWriteRepository : IQueueWriteRepository
    {
        private readonly IDatabase _database;
        private readonly ILogger<QueueWriteRepository> _logger;
        private readonly string ProductUpdateQueueName = "products:updates:pending";
        private readonly string ProductTimestamps = "products:updates:timestamps";
        private readonly string ProductQueueList = "products:updates:list";
        private readonly string ProductAttributeChanges = "products:updates:attributes";

        public QueueWriteRepository(IConnectionMultiplexer redis, ILogger<QueueWriteRepository> logger)
        {
            _database = redis.GetDatabase();
            _logger = logger;
        }

        public async Task AddToQueueAsync(IEnumerable<string> ids)
        {
            var changes = ids.Select(id => new EntityItem
            {
                Id = id,
                Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
            });

            await AddEntityUpdatesToQueueAsync(changes);
        }

        public async Task AddEntityUpdatesToQueueAsync(IEnumerable<EntityItem> changes)
        {
            var changesList = changes.ToList();

            if (!changesList.Any())
            {
                _logger.LogWarning("AddToQueueAsync called uden ændringer i listen");
                return;
            }


            var tasks = new List<Task>();

            foreach (var change in changesList)
            {
                var id = change.Id;
                long timestamp = change.Timestamp > 0 ? change.Timestamp : DateTimeOffset.Now.ToUnixTimeMilliseconds();

                // Add to set (Flag for new entry af entities)
                var addedFlag = await _database.SetAddAsync(ProductUpdateQueueName, id);

                if (addedFlag)
                {
                    _logger.LogDebug("Product {ProductId} added to queue (new entry).", id);

                    // Store timestamp
                    tasks.Add(_database.SortedSetAddAsync(ProductTimestamps, id, timestamp));

                    // Store attribute changes som Json hvis der findes ændringer
                    if (change.ChangedAttributes != null && change.ChangedAttributes.Any())
                    {
                        string attributesJson = JsonSerializer.Serialize(change.ChangedAttributes);
                        tasks.Add(_database.StringSetAsync($"{ProductAttributeChanges}:{id}", attributesJson));

                        _logger.LogDebug("Product {ProductId} has {Count} attribute changes stored in Redis.",
                            id, change.ChangedAttributes.Count);
                    }

                    // Add productId til Redis Queue List
                    tasks.Add(_database.ListLeftPushAsync(ProductQueueList, id));
                }
                else
                {
                    _logger.LogDebug("Product {ProductId} already exists in queue (duplicate skipped).", id);
                }
            }

            if (tasks.Any())
            {
                await Task.WhenAll(tasks);
                _logger.LogInformation("Successfully added {Count} products to Redis queue.", changesList.Count);
            }
        }

        public async Task RemoveFromQueueAsync(IEnumerable<string> ids)
        {
            RedisValue[] redisValues = ids.Select(id => (RedisValue)id).ToArray();

            if (redisValues.Length == 0)
            {
                return;
            }

            _logger.LogInformation("Removing {Count} products from Redis queue...", redisValues.Length);

            var tasks = new List<Task>
            {
                _database.SetRemoveAsync(ProductUpdateQueueName, redisValues),
                _database.SortedSetRemoveAsync(ProductTimestamps, redisValues)
            };

            // Remove attribute changes
            foreach (var id in ids)
            {
                tasks.Add(_database.KeyDeleteAsync($"{ProductAttributeChanges}:{id}"));
                _logger.LogDebug("Removing attribute data for product {ProductId}.", id);
            }

            await Task.WhenAll(tasks);

            _logger.LogInformation("Successfully removed {Count} products from Redis queue.", redisValues.Length);
        }

        public async Task RequeueIdsAsync(IEnumerable<string> ids)
        {
            if (!ids.Any())
            {
                return;
            }

            RedisValue[] redisValues = ids.Select(id => (RedisValue)id).ToArray();

            await _database.ListRightPushAsync(ProductQueueList, redisValues);
        }
    }
}
