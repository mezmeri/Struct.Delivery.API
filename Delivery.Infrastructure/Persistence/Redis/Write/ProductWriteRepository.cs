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

        public ProductWriteRepository(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
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
