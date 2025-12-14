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
        private const string _hashKey = "products:cached";

        public ProductWriteRepository(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task AddToCacheAsync(IEnumerable<ProductModel> products)
        {
            IBatch batch = _database.CreateBatch();

            List<Task<bool>> tasks = products.Select(p => batch.HashSetAsync(_hashKey, p.Id.ToString(), JsonSerializer.Serialize(p))).ToList();

            batch.Execute();
            await Task.WhenAll(tasks);
        }

        public async Task UpdateToCacheAsync(IEnumerable<ProductModel> products)
        {
            IBatch batch = _database.CreateBatch();

            List<Task<bool>> tasks = products.Select(p => batch.HashSetAsync(_hashKey, p.Id.ToString(), JsonSerializer.Serialize(p))).ToList();

            batch.Execute();
            await Task.WhenAll(tasks);
        }

        public async Task DeleteFromCacheAsync(IEnumerable<string> ids)
        {
            IBatch batch = _database.CreateBatch();
            List<Task> tasks = new List<Task>();

            foreach (var id in ids)
            {
                tasks.Add(batch.HashDeleteAsync(_hashKey, id));
            }

            batch.Execute();
            await Task.WhenAll(tasks);
        }
    }
}
