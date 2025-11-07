using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Delivery.Application.Interfaces;

namespace Delivery.Infrastructure.Persistence
{
    public class WriteProductRepository : IProductWriteRepository
    {
        private readonly IDatabase _database;
        private const string ProductUpdateQueueName = "products:updates:pending";

        public WriteProductRepository(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public Task AddToQueueAsync(string id)
        {
            return _database.SetAddAsync(ProductUpdateQueueName, id);
        }

        public Task AddToQueueAsync(IEnumerable<string> ids)
        {
            RedisValue[] redisValues = ids.Select(id => (RedisValue)id).ToArray();

            if (redisValues.Length == 0)
            {
                return Task.CompletedTask;
            }

            return _database.SetAddAsync(ProductUpdateQueueName, redisValues);
        }
    }
}
