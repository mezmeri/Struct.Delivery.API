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

        //Fixed, bruger nu hashKey til at læse fra Redis
        public async Task<Dictionary<string, string>> GetCachedItemsAsync(string hashKey = "products:cached")
        {
            var results = new Dictionary<string, string>();
            var entries = await _database.HashGetAllAsync(hashKey);

            foreach (var entry in entries)
            {
                results[entry.Name.ToString()] = entry.Value.ToString();
            }

            return results;
        }

        // Get all items fra vores Queue-List
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

        //Get all entries fra hashmap m. timestamp
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

        // Bruges til Case 4 - få længde af queue og idmap (hurtig lookup)
        public async Task<(long queueLength, long mapSize)> GetQueueStatsAsync()
        {
            var queueLength = await _database.ListLengthAsync("queue:events");
            var mapSize = await _database.HashLengthAsync("queue:idmap");

            return (queueLength, mapSize);
        }
    }
}
