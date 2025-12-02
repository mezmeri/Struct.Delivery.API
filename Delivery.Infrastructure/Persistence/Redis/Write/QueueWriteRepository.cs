using Delivery.Application.Interfaces.Repositories;
using StackExchange.Redis;
using Delivery.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Delivery.Domain.Events;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace Delivery.Infrastructure.Persistence.Redis.Write
{
    public class QueueWriteRepository : IQueueWriteRepository
    {
        private readonly IDatabase _database;

        private string _listKey = "queue:events";
        private string _sortedSetKey = "queue:events:timestamps";

        public QueueWriteRepository(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task AddToQueueAsync(IEnumerable<QueueItemEventArgs> events)
        {
            long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            SortedSetEntry[] sortedSetEntries = events.Select(x =>
            {
                x.Timestamp = timestamp;
                return new SortedSetEntry(JsonSerializer.Serialize(x), timestamp);
            }).ToArray();

            bool[] addedFlags = await Task.WhenAll(sortedSetEntries.Select(entry => _database.SortedSetAddAsync(_sortedSetKey, entry.Element, entry.Score)));

            List<RedisValue> newIds = new List<RedisValue>();

            for (int i = 0; i < sortedSetEntries.Length; i++)
            {
                if (addedFlags[i])
                    newIds.Add(sortedSetEntries[i].Element);
            }

            if (newIds.Count > 0)
            {
                await _database.ListRightPushAsync(_listKey, newIds.ToArray());
            }

        }

        public async Task RemoveFromQueueAsync(IEnumerable<string> ids)
        {

            if (!ids.Any())
            {
                return;
            }

            RedisValue[] redisValues = ids.Select(id => (RedisValue)id).ToArray();
            
            await _database.SortedSetRemoveAsync(_sortedSetKey, redisValues);
        }

        public async Task RequeueItemsAsync(IEnumerable<QueueItemEventArgs> items)
        {
            if (!items.Any())
            {
                return;
            }

            List<RedisValue> serializedItems = new List<RedisValue>();

            foreach (var item in items)
            {
                double? existingScore = await _database.SortedSetScoreAsync(_sortedSetKey, JsonSerializer.Serialize(item));

                if (!existingScore.HasValue) continue;

                item.Timestamp = (long)existingScore.Value;

                serializedItems.Add(JsonSerializer.Serialize(item));
            }

            if (serializedItems.Count > 0)
            {
                await _database.ListRightPushAsync(_listKey, serializedItems.ToArray());
            }
        }

    }
}
