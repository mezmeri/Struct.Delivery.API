using Delivery.Application.Interfaces.Repositories;
using Delivery.Application.Services;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Struct.App.Api.Models;
using Struct.App.Api.Models.Product;
using System.Text.Json;

namespace Delivery.Infrastructure.Persistence.Redis.Read
{
    public class ProductReadRepository : IProductReadRepository
    {
        private readonly IDatabase _database;
        private PimApiService _pimApiService;
        private readonly string ProductQueueName = "products:updates:pending";
        private readonly string ProductTimestamps = "products:updates:timestamps";

        public ProductReadRepository(IConnectionMultiplexer redis, PimApiService pimApiService)
        {
            _database = redis.GetDatabase();
            _pimApiService = pimApiService;
        }

        public async Task CacheUpdates(IEnumerable<ProductModel> products)
        {
            IBatch batch = _database.CreateBatch();
            List<Task> tasks = new List<Task>();

            foreach (var product in products)
            {
                string key = $"products:{product.Id}:cached";
                string value = JsonSerializer.Serialize(product);
                tasks.Add(batch.StringSetAsync(key, value, TimeSpan.FromHours(1)));
            }
            batch.Execute();
            await Task.WhenAll(tasks);


        }

        public async Task<IEnumerable<(string, long)>> GetProductChanges()
        {
            SortedSetEntry[] entries = await _database.SortedSetRangeByRankWithScoresAsync(ProductTimestamps);
            List<(string, long)> list = entries.Select(e => (e.Element.ToString(), (long)e.Score)).ToList();

            return list;
        }

        public async Task RemoveFromQueueAsync(IEnumerable<string> ids)
        {
            RedisValue[] redisValues = ids.Select(id => (RedisValue)id).ToArray();

            if (redisValues.Length == 0)
            {
                return;
            }

            await _database.SetRemoveAsync(ProductQueueName, redisValues);
            await _database.SortedSetRemoveAsync(ProductTimestamps, redisValues);
        }

        public async Task<IEnumerable<ProductModel>> GetPimData(IEnumerable<string> ids)
        {
            var productIds = ids.Select(id => int.TryParse(id, out var pid) ? (int?)pid : null).OfType<int>().ToList();
            if (!productIds.Any()) return Enumerable.Empty<ProductModel>();
            return await _pimApiService.GetProductDataAsync(productIds);
        }
    }
}
