using Delivery.Application.Interfaces.Repositories;
using Delivery.Application.Services;
using Delivery.Domain.Events;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Delivery.Infrastructure.Persistence.Redis.Write
{
    public class QueueWriteRepository : IQueueWriteRepository
    {
        private readonly IDatabase _database;
        private readonly ILogger<QueueWriteRepository> _logger;
        private string _listKey = "queue:events";
        private string _hashSetKey = "queue:idmap";

        public QueueWriteRepository(IConnectionMultiplexer redis, ILogger<QueueWriteRepository> logger)
        {
            _database = redis.GetDatabase();
            _logger = logger;
        }

        public async Task AddToQueueAsync(IEnumerable<QueueItemDTO> events)
        {
            long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            List<RedisValue> itemsToPush = new List<RedisValue>();

            foreach (var singularEvent in events)
            {
                singularEvent.Timestamp = timestamp;

                string jsonEvent = JsonSerializer.Serialize(singularEvent);

                await _database.HashSetAsync(_hashSetKey, singularEvent.Id, jsonEvent);

                itemsToPush.Add(jsonEvent);

                _logger.LogInformation($"Added/Updated event for ID {singularEvent.Id} at timestamp {timestamp}");
            }


            if (itemsToPush.Count > 0)
            {
                await _database.ListRightPushAsync(_listKey, itemsToPush.ToArray());

                _logger.LogInformation($"Pushed {itemsToPush.Count()} items to list");
            }

        }

        public async Task RemoveFromQueueAsync(IEnumerable<QueueItemDTO> processedItems)
        {
            IEnumerable<IGrouping<string, QueueItemDTO>> grouped = processedItems.GroupBy(p => p.Id);

            foreach (var group in grouped)
            {
                string id = group.Key;
                long processedTimestamp = group.Max(x => x.Timestamp);

                RedisValue latestJsonItem = await _database.HashGetAsync(_hashSetKey, id);

                QueueItemDTO latestItem = JsonSerializer.Deserialize<QueueItemDTO>(latestJsonItem);

                if (latestItem.Timestamp > processedTimestamp)
                {
                    await _database.ListRightPushAsync(_listKey, latestJsonItem);
                    _logger.LogInformation($"Requeued newer version of ID {id} with timestamp {latestItem.Timestamp}");
                }
                else
                {
                    await _database.HashDeleteAsync(_hashSetKey, id);
                    _logger.LogInformation($"Removed processed ID {id} from hash");
                }
            }
        }

        public async Task RequeueItemsAsync(IEnumerable<QueueItemDTO> items)
        {
            IEnumerable<string> ids = items.Select(i => i.Id).Distinct();
            List<RedisValue> toPush = new List<RedisValue>();

            foreach (var id in ids)
            {
                var latestJsonItem = await _database.HashGetAsync(_hashSetKey, id);

                if (!latestJsonItem.IsNullOrEmpty)
                {
                    toPush.Add(latestJsonItem);

                    _logger.LogInformation($"Requeued latest item for ID {id}");
                }
            }

            if (toPush.Count > 0)
            {
                await _database.ListRightPushAsync(_listKey, toPush.ToArray());
            }
        }

    }
}
