using Delivery.Application.Interfaces.Repositories;
using Delivery.Domain.DTO;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Infrastructure.Persistence.Redis.Write
{
    public class VariantGroupWriteRepository : IVariantGroupWriteRepository
    {
        private readonly IDatabase _database;
        private const string _hashKey = "variantGroups:cached";

        public VariantGroupWriteRepository(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task AddToCacheAsync(IEnumerable<VariantGroupWithAttributesDTO> variantGroups)
        {
            IBatch batch = _database.CreateBatch();

            List<Task<bool>> tasks = variantGroups.Select(p => batch.HashSetAsync(_hashKey, p.VariantGroup.Id.ToString(), JsonConvert.SerializeObject(p))).ToList();

            batch.Execute();
            await Task.WhenAll(tasks);
        }

        public async Task UpdateToCacheAsync(IEnumerable<VariantGroupWithAttributesDTO> variantGroups)
        {
            IBatch batch = _database.CreateBatch();

            List<Task<bool>> tasks = variantGroups.Select(p => batch.HashSetAsync(_hashKey, p.VariantGroup.Id.ToString(), JsonConvert.SerializeObject(p))).ToList();

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
