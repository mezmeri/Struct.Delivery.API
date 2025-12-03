using Delivery.Application.Interfaces.Repositories;
using Delivery.Application.Services;
using Delivery.Domain.Events;
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

        private string _listKey = "queue:events";
        private string _sortedSetKey = "queue:events:timestamps";

        public QueueReadRepository(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task<IEnumerable<QueueItemEventArgs>> GetQueueUpdates(int batchSize = 100)
        {
            List<QueueItemEventArgs> events = new List<QueueItemEventArgs>();

            for (int i = 0; i < batchSize; i++)
            {
                RedisValue item = await _database.ListLeftPopAsync(_listKey);

                if (!item.HasValue)
                {
                    break;
                }

                QueueItemEventArgs? queueItem = JsonSerializer.Deserialize<QueueItemEventArgs>(item);
                if (queueItem != null)
                {
                    events.Add(queueItem);
                }
            }
            
            return events;
        }

        public async Task<Dictionary<string, long>> GetLatestTimestampsAsync(IEnumerable<string> ids)
        {
            Dictionary<string, long> results = new Dictionary<string, long>();

            foreach (string id in ids)
            {
                double? score = await _database.SortedSetScoreAsync(_sortedSetKey, id);

                if (score.HasValue)
                {
                    results[id] = (long)score.Value;
                }
            }

            return results;
        }

    }
}
