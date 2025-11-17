using Delivery.Application.Interfaces.Repositories;
using StackExchange.Redis;
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
        private readonly string ProductUpdateQueueName = "products:updates:pending";
        private readonly string ProductTimestamps = "products:updates:timestamps";
        private readonly string ProductQueueList = "products:updates:list";

        public QueueWriteRepository(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task AddToQueueAsync(IEnumerable<string> ids)
        {
            long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            RedisValue[] redisValues = ids.Select(id => (RedisValue)id).ToArray();

            if (redisValues.Length == 0)
            {
                return;
            }

            bool[] addedFlags = await Task.WhenAll(redisValues.Select(id => _database.SetAddAsync(ProductUpdateQueueName, id)));

            List<RedisValue> newIds = new List<RedisValue>();

            for (int i = 0; i < redisValues.Length; i++)
            {
                if (addedFlags[i])
                    newIds.Add(redisValues[i]);
            }

            var entries = redisValues.Select(id => new SortedSetEntry(id, timestamp)).ToArray();
            await _database.SortedSetAddAsync(ProductTimestamps, entries);

            await _database.ListLeftPushAsync(ProductQueueList, newIds.ToArray());

        d}
  
    }
}
