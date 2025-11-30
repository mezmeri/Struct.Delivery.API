using Delivery.Application.Interfaces.Repositories;
using StackExchange.Redis;
using Struct.App.Api.Models.Variant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Infrastructure.Persistence.Redis.Write
{
    public class VariantWriteRepository : IVariantWriteRepository
    {
        private readonly IDatabase _database;

        public VariantWriteRepository(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task CacheUpdates(IEnumerable<VariantModel> variants)
        {
            IBatch batch = _database.CreateBatch();
            List<Task> tasks = new List<Task>();

            foreach (var variant in variants)
            {
                string key = $"variants:{variant.Id}:cached";
                string value = System.Text.Json.JsonSerializer.Serialize(variant);
                tasks.Add(batch.StringSetAsync(key, value, TimeSpan.FromHours(1)));
            }
            batch.Execute();
            await Task.WhenAll(tasks);
        }
    }
}
