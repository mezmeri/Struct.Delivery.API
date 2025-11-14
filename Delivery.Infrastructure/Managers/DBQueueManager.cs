using Delivery.Application.Interfaces.Managers;
using Delivery.Application.Interfaces.Repositories;
using Delivery.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Infrastructure.Managers
{
    public class DBQueueManager : IDbQueueManager
    {
        IProductWriteRepository _productWriterRepository;
        IProductReadRepository _productReadRepository;
        FilterDirtyIdsService _filterDirtyIdsService;

        public DBQueueManager(IProductWriteRepository productWriteRepository, IProductReadRepository productReadRepository, FilterDirtyIdsService filterDirtyIdsService) 
        {
            _productWriterRepository = productWriteRepository;
            _productReadRepository = productReadRepository;
            _filterDirtyIdsService = filterDirtyIdsService;
        }

        public async Task EnqueueUpdatesAsync(IEnumerable<string> ids)
        {
            await _productWriterRepository.AddToQueueAsync(ids);

            await ProcessQueueAsync();
        }

        public async Task ProcessQueueAsync()
        {
            IEnumerable<(string, long)> queuedChanges = await _productReadRepository.GetProductChanges();

            if (!queuedChanges.Any())
            {
                return;  
            }

            IEnumerable<string> cleanIds = await _filterDirtyIdsService.FilterDirtyIds(queuedChanges);

            if (!cleanIds.Any())
            {
                return;
            }

            var data = await _productReadRepository.GetPimData(cleanIds);

            await _productReadRepository.CacheUpdates(data);

            await _productReadRepository.RemoveFromQueueAsync(cleanIds);

        }
    }
}
