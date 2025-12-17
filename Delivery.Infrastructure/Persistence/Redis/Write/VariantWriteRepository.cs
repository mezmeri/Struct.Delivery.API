using Delivery.Application.Interfaces.Repositories;
using Delivery.Domain.DTO;
using Newtonsoft.Json;
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
        private const string _hashKey = "variants:cached";

        public VariantWriteRepository(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task AddToCacheAsync(IEnumerable<VariantWithAttributesDTO> variants)
        {
            IBatch batch = _database.CreateBatch();

            List<Task<bool>> tasks = variants.Select(v => batch.HashSetAsync(_hashKey, v.Variant.Id.ToString(), JsonConvert.SerializeObject(v))).ToList();

            batch.Execute();
            await Task.WhenAll(tasks);
        }

        public async Task UpdateToCacheAsync(IEnumerable<VariantWithAttributesDTO> variants)
        {
            IBatch batch = _database.CreateBatch();
            List<Task<bool>> tasks = variants.Select(v => batch.HashSetAsync(_hashKey, v.Variant.Id.ToString(), JsonConvert.SerializeObject(v))).ToList();
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
