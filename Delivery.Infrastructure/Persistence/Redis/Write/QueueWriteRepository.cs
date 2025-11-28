using Delivery.Application.Interfaces.Repositories;
using StackExchange.Redis;
using Delivery.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Infrastructure.Persistence.Redis.Write
{
    public class QueueWriteRepository : IQueueWriteRepository
    {
        private readonly IDatabase _database;
        private readonly GenerateKeyService _keyService;
        private readonly string ProductUpdateQueueName = "products:updates:pending";
        private readonly string ProductTimestamps = "products:updates:timestamps";
        private readonly string ProductQueueList = "products:updates:list";

        public QueueWriteRepository(IConnectionMultiplexer redis, GenerateKeyService keyService)
        {
            _database = redis.GetDatabase();
            _keyService = keyService;
        }

        public async Task AddToQueueAsync(string eventType, IEnumerable<string> ids)
        {
            var (setKey, timestampKey, listKey) = _keyService.GenerateQueueKey(eventType);

            long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            RedisValue[] redisValues = ids.Select(id => (RedisValue)id).ToArray();

            if (redisValues.Length == 0)
            {
                return;
            }

            bool[] addedFlags = await Task.WhenAll(redisValues.Select(id => _database.SetAddAsync(setKey, id)));

            List<RedisValue> newIds = new List<RedisValue>();

            for (int i = 0; i < redisValues.Length; i++)
            {
                if (addedFlags[i])
                    newIds.Add(redisValues[i]);
            }

            var entries = redisValues.Select(id => new SortedSetEntry(id, timestamp)).ToArray();

            await _database.SortedSetAddAsync(timestampKey, entries);

            await _database.ListLeftPushAsync(listKey, newIds.ToArray());

            await _database.SetAddAsync("queues:all", listKey);


        }

        public async Task RemoveFromQueueAsync(string eventType, IEnumerable<string> ids)
        {
            RedisValue[] redisValues = ids.Select(id => (RedisValue)id).ToArray();

            if (redisValues.Length == 0)
            {
                return;
            }

            var (setKey, timestampKey, listKey) = _keyService.GenerateQueueKey(eventType);

            await _database.SetRemoveAsync(setKey, redisValues);
            await _database.SortedSetRemoveAsync(timestampKey, redisValues);
        }

        public async Task RequeueIdsAsync(string eventType, IEnumerable<string> ids)
        {
            if (!ids.Any())
            {
                return;
            }

            var (setKey, timestampKey, listKey) = _keyService.GenerateQueueKey(eventType);

            RedisValue[] redisValues = ids.Select(id => (RedisValue)id).ToArray();

            await _database.ListRightPushAsync(listKey, redisValues);
        }

    }
}
