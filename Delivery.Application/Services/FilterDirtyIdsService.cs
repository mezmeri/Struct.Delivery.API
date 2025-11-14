using Delivery.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Application.Services
{
    public class FilterDirtyIdsService
    {
        private readonly IProductReadRepository _productReadRepository;
        public FilterDirtyIdsService(IProductReadRepository productReadRepository)
        {
            _productReadRepository = productReadRepository;
        }

        public async Task<IEnumerable<string>> FilterDirtyIds(IEnumerable<(string Id, long Timestamp)> idsWithTimestamps)
        {
            
            IEnumerable<(string, long)> latestQueued = await _productReadRepository.GetProductChanges();

            Dictionary<string, long> latestDictionary = latestQueued.ToDictionary(x => x.Item1, x => x.Item2);

            return idsWithTimestamps
                .Where(x => latestDictionary.TryGetValue(x.Id, out var latest) && x.Timestamp >= latest)
                .Select(x => x.Id);
        }
    }
}