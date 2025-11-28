using Delivery.Application.Interfaces.Repositories;
using Delivery.Application.Services;
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
        private readonly GenerateKeyService _generateKeyService;

        public QueueReadRepository(IConnectionMultiplexer redis, GenerateKeyService generateKeyService)
        {
            _database = redis.GetDatabase();
            _generateKeyService = generateKeyService;
        }

        public async Task<IEnumerable<(string, long, string)>> GetQueueUpdates(int batchSize = 100)
        {
            var result = new List<(string, long, string)>();

            string[] allQueueKeys = (await _database.SetMembersAsync("queues:all")).Select(x => x.ToString()).ToArray();

            foreach (string key in allQueueKeys)
            {
                for (int i = 0; i < batchSize; i++)
                {
                    string eventType = _generateKeyService.ExtractEventType(key);

                    RedisValue item = await _database.ListLeftPopAsync(key);

                    if (!item.HasValue)
                    {
                        break;
                    }

                    string timestampKey = key.Replace(":list", ":timestamps");

                    long timestamp = (long)(await _database.SortedSetScoreAsync(timestampKey, item)).GetValueOrDefault();
                    
                    result.Add((item, timestamp, eventType));
                }
            }

            return result;
        }

    }
}
