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

            var addTasks = redisValues.Select(id => _database.SortedSetAddAsync(ProductTimestamps, id, timestamp)).ToArray();

            bool[] addedFlags = await Task.WhenAll(addTasks);

            List<RedisValue> newIds = new List<RedisValue>();

            for (int i = 0; i < addedFlags.Length; i++)
            {
                if (addedFlags[i])
                {
                    newIds.Add(redisValues[i]);
                }
            }

            if (newIds.Count > 0)
            {
                await _database.ListLeftPushAsync(ProductQueueList, newIds.ToArray());
            }

        }

        public async Task RemoveFromQueueAsync(IEnumerable<(string, long)> productUpdates)
        {
            List<(string, long)> updatesList = productUpdates.ToList();
            
            if (updatesList.Count == 0)
            {
                return;
            }

            RedisValue[] redisValues = updatesList.Select(u => (RedisValue)u.Item1).ToArray();

            Task<double?>[] scoreTasks = updatesList.Select(u => _database.SortedSetScoreAsync(ProductTimestamps, u.Item1)).ToArray();
            
            double?[] redisScores = await Task.WhenAll(scoreTasks);

            List<RedisValue> toRequeue = new List<RedisValue>();

            for (int i = 0; i < updatesList.Count; i++)
            {
                if (redisScores[i].HasValue && redisScores[i].Value > updatesList[i].Item2)
                {
                    toRequeue.Add(updatesList[i].Item1);
                }
            }

            await _database.SortedSetRemoveAsync(ProductTimestamps, redisValues);

            if (toRequeue.Count > 0)
            {
                await _database.ListLeftPushAsync(ProductQueueList, toRequeue.ToArray());
            }
        }

        public async Task RequeueIdsAsync(IEnumerable<string> ids)
        {
            if (!ids.Any())
            {
                return;
            }

            RedisValue[] redisValues = ids.Select(id => (RedisValue)id).ToArray();

            await _database.ListRightPushAsync(ProductQueueList, redisValues);
        }

    }
}
