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
        private readonly IQueueReadRepository _queueReadRepository;
        private readonly IProductWriteRepository _productWriteRepository;
        private readonly IProductReadRepository _productReadRepository;
        private readonly PimApiService _pimApiService;
        private readonly FilterDirtyIdsService _filterDirtyIdsService;

        public QueueManager(IQueueWriteRepository queueWriteRepository, IQueueReadRepository queueReadRepository, IProductWriteRepository productWriteRepository, IProductReadRepository productReadRepository, FilterDirtyIdsService filterDirtyIdsService, PimApiService pimApiService)
        {
            _queueWriteRepository = queueWriteRepository;
            _queueReadRepository = queueReadRepository;
            _productWriteRepository = productWriteRepository;
            _productReadRepository = productReadRepository;
            _filterDirtyIdsService = filterDirtyIdsService;
            _pimApiService = pimApiService;
        }

        public async Task EnqueueUpdatesAsync(string eventType, IEnumerable<string> ids)
        {
            await _queueWriteRepository.AddToQueueAsync(eventType, ids);

            await ProcessQueueAsync();
        }

        public async Task ProcessQueueAsync()
        {
            IEnumerable<(string, long, string)> queuedChanges = await _queueReadRepository.GetQueueUpdates(100);

            if (!queuedChanges.Any())
            {
                return;
            }

            IEnumerable<string> cleanIds = await _filterDirtyIdsService.FilterDirtyIds(queuedChanges);

            List<string> dirtyIds = queuedChanges.Select(x => x.Item1).Except(cleanIds).ToList();


            if (dirtyIds.Any())
            {
                var dirtyGroups = queuedChanges.Where(x => dirtyIds.Contains(x.Item1)).GroupBy(x => x.Item3); 

                foreach (var group in dirtyGroups)
                {
                    string eventType = group.Key;
                    List<string> ids = group.Select(x => x.Item1).ToList();

                    await _queueWriteRepository.RequeueIdsAsync(eventType, ids);
                }
            }

            if (cleanIds.Any())
            {
                var groupedEvents = queuedChanges.Where(x => cleanIds.Contains(x.Item1)).GroupBy(x => x.Item3);

                foreach (var group in groupedEvents)
                {
                    string eventType = group.Key;
                    List<string> ids = group.Select(x => x.Item1).ToList();

                    switch (eventType)
                    {
                        case "products:updated":
                            var products = await _pimApiService.GetProductDataAsync(ids);
                            await _productWriteRepository.CacheUpdates(products);
                            break;
                        default:
                            break;
                    }

                    await _queueWriteRepository.RemoveFromQueueAsync(eventType, ids);
                }
            }
        } 
            
    }
}
