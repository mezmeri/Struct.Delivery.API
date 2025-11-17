using Delivery.Application.Interfaces.Repositories;
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
        private readonly string ProductUpdateQueueName = "products:updates:pending";
        private readonly string ProductTimestamps = "products:updates:timestamps";
        private readonly string ProductQueueList = "products:updates:list";

        public QueueReadRepository(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task<IEnumerable<(string, long)>> GetProductChanges(int batchSize)
        {
            var result = new List<(string, long)>();

            RedisValue[] items = await _database.ListRangeAsync(ProductQueueList, 0, batchSize - 1);

            foreach (var item in items)
            {
                long timestamp = (long)(await _database.SortedSetScoreAsync(ProductTimestamps, item)).GetValueOrDefault();
                result.Add((item, timestamp));
            }

            return result;
        }

        public async Task RemoveFromQueueAsync(IEnumerable<string> ids)
        {
            RedisValue[] redisValues = ids.Select(id => (RedisValue)id).ToArray();

            if (redisValues.Length == 0)
            {
                return;
            }

            foreach (var id in redisValues)
            {
                await _database.ListRemoveAsync(ProductQueueList, id, 1);
            }

            await _database.SetRemoveAsync(ProductUpdateQueueName, redisValues);
            await _database.SortedSetRemoveAsync(ProductTimestamps, redisValues);
        }
    }
}
