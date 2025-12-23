using Delivery.Application.Interfaces.Repositories;
using Delivery.Application.Services;
using Delivery.Domain.Events;
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

        private string _listKey = "queue:events";
        private string _hashSetKey = "queue:idmap";
        private readonly ILogger<QueueReadRepository> _logger;

        public QueueReadRepository(IConnectionMultiplexer redis, ILogger<QueueReadRepository> logger)
        {
            _database = redis.GetDatabase();
            _logger = logger;
        }

        public async Task <IEnumerable<QueueItemDTO>> GetQueueUpdates(int batchSize = 100)
        {
            List<QueueItemDTO> events = new List<QueueItemDTO>();

            for (int i = 0; i < batchSize; i++)
            {
                RedisValue item = await _database.ListLeftPopAsync(_listKey);

                if (!item.HasValue)
                {
                    break;
                }

                QueueItemDTO? queueItem = JsonSerializer.Deserialize<QueueItemDTO>(item);
                if (queueItem != null)
                {
                    events.Add(queueItem);
                }
            }

            _logger.LogInformation($"Popped {events.Count()} items from queue");

            return events;
        }

        public async Task<Dictionary<string, long>> GetLatestTimestampsAsync(IEnumerable<string> ids)
        {
            Dictionary<string, long> results = new Dictionary<string, long>();

            foreach (string id in ids)
            {
                RedisValue json = await _database.HashGetAsync(_hashSetKey, id);
                if (!json.IsNullOrEmpty)
                {
                    var obj = JsonSerializer.Deserialize<QueueItemDTO>(json);
                    if (obj != null)
                    {
                        results[id] = obj.Timestamp;
                    }
                }
            }

            _logger.LogInformation($"Latest timestamp retrieved");

            return results;
        }

        //Peek without popping - Brugt til monitorering inden caching
        public async Task<List<QueueItemDTO>> PeekQueueItemsAsync(int count = 100)
        {
            RedisValue[] items = await _database.ListRangeAsync(_listKey, 0, count - 1);

            return items
                .Where(item => item.HasValue)
                .Select(item => JsonSerializer.Deserialize<QueueItemDTO>(item))
                .Where(item => item != null)
                .ToList();
        }
    }
}