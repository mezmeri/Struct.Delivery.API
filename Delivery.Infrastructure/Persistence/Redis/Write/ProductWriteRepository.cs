using Delivery.Application.Interfaces.Repositories;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Infrastructure.Persistence.Redis.Write
{
    public class ProductWriteRepository : IProductWriteRepository
    {
        private readonly IDatabase _database;
        private readonly string ProductUpdateQueueName = "products:updates:pending";
        private readonly string ProductTimestamps = "products:updates:timestamps";

        public ProductWriteRepository(IConnectionMultiplexer redis)
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

            await _database.SetAddAsync(ProductUpdateQueueName, redisValues);

            var entries = redisValues.Select(id => new SortedSetEntry(id, timestamp)).ToArray();
            await _database.SortedSetAddAsync(ProductTimestamps, entries);

        }
    }
}
