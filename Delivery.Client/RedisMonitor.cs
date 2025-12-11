using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Client
{
    public class RedisMonitor
    {
        private readonly IDatabase _database;
        private readonly IServer _server;

        public RedisMonitor(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
            _server = redis.GetServer(redis.GetEndPoints().First());
        }

        /// <summary>
        /// Gets all cached items matching a pattern (default: products)
        /// </summary>
        public async Task<Dictionary<string, string>> GetCachedItemsAsync(string pattern = "products:*:cached")
        {
            var results = new Dictionary<string, string>();
            var keys = _server.Keys(pattern: pattern);

            foreach (var key in keys)
            {
                var value = await _database.StringGetAsync(key);
                if (!value.IsNullOrEmpty)
                {
                    results[key.ToString()] = value.ToString();
                }
            }

            return results;
        }

        /// <summary>
        /// Gets all items from the queue list (ready to be processed)
        /// </summary>
        public async Task<List<string>> GetQueueItemsAsync(string queueKey = "queue:events")
        {
            var items = new List<string>();
            var values = await _database.ListRangeAsync(queueKey);

            foreach (var value in values)
            {
                if (!value.IsNullOrEmpty)
                {
                    items.Add(value.ToString());
                }
            }

            return items;
        }

        /// <summary>
        /// Gets all entries from the queue ID map (latest timestamps)
        /// </summary>
        public async Task<Dictionary<string, string>> GetQueueIdMapAsync(string hashKey = "queue:idmap")
        {
            var results = new Dictionary<string, string>();
            var entries = await _database.HashGetAllAsync(hashKey);

            foreach (var entry in entries)
            {
                results[entry.Name.ToString()] = entry.Value.ToString();
            }

            return results;
        }

        /// <summary>
        /// Gets basic statistics about the queue
        /// </summary>
        public async Task<(long queueLength, long mapSize)> GetQueueStatsAsync()
        {
            var queueLength = await _database.ListLengthAsync("queue:events");
            var mapSize = await _database.HashLengthAsync("queue:idmap");

            return (queueLength, mapSize);
        }
    }
}
