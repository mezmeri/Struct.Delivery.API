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
        private readonly IQueueWriteRepository _queueWriteRepository;

        // MOVE TO Domain -> QueueWorker -> Product

        //private readonly IQueueReadRepository _queueReadRepository;
        //private readonly IProductWriteRepository _productWriteRepository;
        //private readonly IProductReadRepository _productReadRepository;
        //private readonly PimApiService _pimApiService;
        //private readonly FilterDirtyIdsService _filterDirtyIdsService;

        public QueueManager(IQueueWriteRepository queueWriteRepository)
            //IQueueReadRepository queueReadRepository,IProductWriteRepository productWriteRepository, IProductReadRepository productReadRepository, FilterDirtyIdsService filterDirtyIdsService, PimApiService pimApiService) 
        {
            _queueWriteRepository = queueWriteRepository;
            
            // MOVE TO Domain -> QueueWorker -> Product

            //_queueReadRepository = queueReadRepository;
            //_productWriteRepository = productWriteRepository;
            //_productReadRepository = productReadRepository;
            //_filterDirtyIdsService = filterDirtyIdsService;
            //_pimApiService = pimApiService;
        }

        public async Task EnqueueUpdatesAsync(IEnumerable<string> ids)
        {
            await _queueWriteRepository.AddToQueueAsync(ids);

            //await ProcessQueueAsync();
        }

        //public async Task ProcessQueueAsync()
        //{
        //    IEnumerable<(string, long)> queuedChanges = await _queueReadRepository.GetProductChanges(100);

        //    if (!queuedChanges.Any())
        //    {
        //        return;  
        //    }

        //    IEnumerable<string> cleanIds = await _filterDirtyIdsService.FilterDirtyIds(queuedChanges);

        //    List<string> dirtyIds = queuedChanges.Select(x => x.Item1).Except(cleanIds).ToList();


        //    if (dirtyIds.Any())
        //    {
        //        await _queueReadRepository.RequeueIdsAsync(dirtyIds);
        //    }

        //    if (cleanIds.Any())
        //    {
        //        var data = await _pimApiService.GetProductDataAsync(cleanIds);

        //        await _productWriteRepository.CacheUpdates(data);

        //        await _queueReadRepository.RemoveFromQueueAsync(cleanIds);
        //    }

        //}
    }
}
