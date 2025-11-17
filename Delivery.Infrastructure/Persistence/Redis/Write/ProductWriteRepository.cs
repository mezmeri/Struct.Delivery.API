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

namespace Delivery.Infrastructure.Persistence.Redis.Write
{
    public class ProductWriteRepository : IProductWriteRepository
    {
        private readonly IDatabase _database;

        public ProductWriteRepository(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
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
    }
}
