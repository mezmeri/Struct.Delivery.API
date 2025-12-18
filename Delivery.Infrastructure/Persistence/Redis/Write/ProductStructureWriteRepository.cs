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
    public class ProductStructureWriteRepository : IProductStructureWriteRepository
    {
        private readonly IDatabase _database;
        private const string _hashKey = "productStructure:cached";

        public ProductStructureWriteRepository(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task AddToCacheAsync(IEnumerable<ProductStructureWithAttributesDTO> productStructures)
        {
            IBatch batch = _database.CreateBatch();

            List<Task<bool>> tasks = productStructures.Select(p => batch.HashSetAsync(_hashKey, p.ProductStructure.Uid.ToString(), JsonConvert.SerializeObject(p))).ToList();

            batch.Execute();
            await Task.WhenAll(tasks);
        }

        public async Task UpdateToCacheAsync(IEnumerable<ProductStructureWithAttributesDTO> productStructures)
        {
            IBatch batch = _database.CreateBatch();

            List<Task<bool>> tasks = productStructures.Select(p => batch.HashSetAsync(_hashKey, p.ProductStructure.Uid.ToString(), JsonConvert.SerializeObject(p))).ToList();

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
