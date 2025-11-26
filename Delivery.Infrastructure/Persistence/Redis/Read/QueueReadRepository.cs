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
        private readonly string _productTimestamps = "products:updates:timestamps";
        private readonly string _productQueueList = "products:updates:list";

        public QueueReadRepository(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task<IEnumerable<(string, long)>> GetProductChanges(int batchSize = 100)
        {
            var result = new List<(string, long)>();

            for (int i = 0; i < batchSize; i++)
            {
                RedisValue item = await _database.ListLeftPopAsync(_productQueueList);

                if (!item.HasValue)
                {
                    break;
                }

                long timestamp = (long)(await _database.SortedSetScoreAsync(_productTimestamps, item)).GetValueOrDefault();
                result.Add((item, timestamp));
            }

            return result;
        }

    }
}
