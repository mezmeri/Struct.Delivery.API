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
    public class QueueManager : IQueueManager
    {
        IQueueWriteRepository _queueWriteRepository;
        IQueueReadRepository _queueReadRepository;
        IProductWriteRepository _productWriteRepository;
        IProductReadRepository _productReadRepository;
        FilterDirtyIdsService _filterDirtyIdsService;

        public QueueManager(IQueueWriteRepository queueWriteRepository, IQueueReadRepository queueReadRepository,IProductWriteRepository productWriteRepository, IProductReadRepository productReadRepository, FilterDirtyIdsService filterDirtyIdsService) 
        {
            _queueWriteRepository = queueWriteRepository;
            _queueReadRepository = queueReadRepository;
            _productWriteRepository = productWriteRepository;
            _productReadRepository = productReadRepository;
            _filterDirtyIdsService = filterDirtyIdsService;
        }

        public async Task EnqueueUpdatesAsync(IEnumerable<string> ids)
        {
            await _queueWriteRepository.AddToQueueAsync(ids);

            await ProcessQueueAsync();
        }

        public async Task ProcessQueueAsync()
        {
            IEnumerable<(string, long)> queuedChanges = await _queueReadRepository.GetProductChanges(100);

            if (!queuedChanges.Any())
            {
                return;  
            }

            IEnumerable<string> cleanIds = await _filterDirtyIdsService.FilterDirtyIds(queuedChanges);

            List<string> dirtyIds = queuedChanges.Select(x => x.Item1).Except(cleanIds).ToList();


            if (dirtyIds.Any())
            {
                await _queueReadRepository.RequeueIdsAsync(dirtyIds);
            }

            if (cleanIds.Any())
            {
                var data = await _productReadRepository.GetPimData(cleanIds);

                await _productWriteRepository.CacheUpdates(data);

                await _queueReadRepository.RemoveFromQueueAsync(cleanIds);
            }

        }
    }
}
