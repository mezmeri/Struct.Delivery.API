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
        private readonly string _productTimestamps = "products:updates:timestamps";
        private readonly string _productQueueList = "products:updates:list";

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

            RedisValue[] newIds = (await Task.WhenAll(redisValues.Select(async id =>
            {
                bool added = await _database.SortedSetAddAsync(_productTimestamps, id, timestamp);
                return (id, added);

            }))).Where(x => x.added).Select(x => x.id).ToArray();

            if (newIds.Length > 0)
            {
                await _database.ListLeftPushAsync(_productQueueList, newIds.ToArray());
            }

        }

        public async Task RemoveFromQueueAsync(IEnumerable<(string, long)> productUpdates)
        {
            List<(string, long)> updatesList = productUpdates.ToList();
            
            if (updatesList.Count == 0)
            {
                return;
            }

            (string, long, double?)[] scores = await Task.WhenAll(
                updatesList.Select(async x =>
                {
                    double? score = await _database.SortedSetScoreAsync(_productTimestamps, x.Item1);
                    return (x.Item1, x.Item2, score);
                })
            );

            RedisValue[] toRequeue = scores.Where(x => x.Item3.HasValue && x.Item3.Value > x.Item2)
                .Select(x => (RedisValue)x.Item1)
                .ToArray();

            RedisValue[] toRemove = scores.Where(x => !x.Item3.HasValue || x.Item3.Value <= x.Item2)
                .Select(x => (RedisValue)x.Item1)
                .ToArray();

            await _database.SortedSetRemoveAsync(_productTimestamps, toRemove);

            if (toRequeue.Length > 0)
            {
                await _database.ListLeftPushAsync(_productQueueList, toRequeue);
            }
        }

        public async Task RequeueIdsAsync(IEnumerable<string> ids)
        {
            if (!ids.Any())
            {
                return;
            }

            RedisValue[] redisValues = ids.Select(id => (RedisValue)id).ToArray();

            await _database.ListRightPushAsync(_productQueueList, redisValues);
        }

    }
}
