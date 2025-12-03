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
        private string _hashSetKey = "queue:idmap";

        public QueueWriteRepository(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task AddToQueueAsync(IEnumerable<QueueItemDTO> events)
        {
            long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            List<RedisValue> newItemsToPush = new List<RedisValue>();

            foreach (var obj in events)
            {
                obj.Timestamp = timestamp;

                string jsonObj = JsonSerializer.Serialize(obj);

                RedisValue oldJson = await _database.HashGetAsync(_hashSetKey, obj.Id);

                bool wasAlreadyQueued = !oldJson.IsNullOrEmpty;

                if (wasAlreadyQueued)
                {
                    await _database.SortedSetRemoveAsync(_sortedSetKey, oldJson);
                }

                await _database.SortedSetAddAsync(_sortedSetKey, jsonObj, timestamp);

                await _database.HashSetAsync(_hashSetKey, obj.Id, jsonObj);

                if (!wasAlreadyQueued)
                {
                    newItemsToPush.Add(jsonObj);
                }
            }


            if (newItemsToPush.Count > 0)
            {
                await _database.ListRightPushAsync(_listKey, newItemsToPush.ToArray());
            }

        }

        public async Task RemoveFromQueueAsync(IEnumerable<string> ids)
        {

            foreach (var id in ids)
            {
                RedisValue jsonObj = await _database.HashGetAsync(_hashSetKey, id);

                await _database.SortedSetRemoveAsync(_sortedSetKey, jsonObj);

                await _database.HashDeleteAsync("queue:idmap", id);
            }
        }

        public async Task RequeueItemsAsync(IEnumerable<QueueItemDTO> items)
        {
            var toPush = new List<RedisValue>();

            foreach (var item in items)
            {
                RedisValue json = await _database.HashGetAsync(_hashSetKey, item.Id);
                if (json.IsNullOrEmpty)
                    continue;

                double? timestamp = await _database.SortedSetScoreAsync(_sortedSetKey, item.Id);
                if (!timestamp.HasValue)
                    continue;

                item.Timestamp = (long)timestamp.Value;

                toPush.Add(json);
            }

            if (toPush.Count > 0)
            {
                await _database.ListRightPushAsync(_listKey, toPush.ToArray());
            }
        }

    }
}
