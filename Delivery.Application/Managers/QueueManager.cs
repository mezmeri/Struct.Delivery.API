using Delivery.Application.Interfaces.Managers;
using Delivery.Application.Interfaces.Repositories;
using Delivery.Application.Services;
using Delivery.Domain.Events;
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
        private readonly IVariantWriteRepository _variantWriteRepository;
        private readonly IVariantReadRepository _variantReadRepository;
        private readonly PimApiService _pimApiService;
        private readonly FilterDirtyIdsService _filterDirtyIdsService;

        public QueueManager(IVariantWriteRepository variantWriteRepository, IVariantReadRepository variantReadRepository, IQueueWriteRepository queueWriteRepository, IQueueReadRepository queueReadRepository, IProductWriteRepository productWriteRepository, IProductReadRepository productReadRepository, FilterDirtyIdsService filterDirtyIdsService, PimApiService pimApiService)
        {
            _variantWriteRepository = variantWriteRepository;
            _variantReadRepository = variantReadRepository;
            _queueWriteRepository = queueWriteRepository;
            _queueReadRepository = queueReadRepository;
            _productWriteRepository = productWriteRepository;
            _productReadRepository = productReadRepository;
            _filterDirtyIdsService = filterDirtyIdsService;
            _pimApiService = pimApiService;
        }

        public async Task EnqueueUpdatesAsync(IEnumerable<QueueItemEventArgs> events)
        {
            await _queueWriteRepository.AddToQueueAsync(events);

            await ProcessQueueAsync();
        }

        public async Task ProcessQueueAsync()
        {
            IEnumerable<QueueItemEventArgs> queuedChanges = await _queueReadRepository.GetQueueUpdates(100);

            if (!queuedChanges.Any())
            {
                return;
            }

            IEnumerable<string> cleanIds = await _filterDirtyIdsService.FilterDirtyIds(queuedChanges);

            List<QueueItemEventArgs> dirtyItems = queuedChanges.Where(x => !cleanIds.Contains(x.Id)).ToList();

            List<QueueItemEventArgs> cleanItems = queuedChanges.Where(x => cleanIds.Contains(x.Id)).ToList();

            if (dirtyItems.Any())
            {
                await _queueWriteRepository.RequeueItemsAsync(dirtyItems);
            }

            if (cleanIds.Any())
            {
                var groupedEvents = cleanItems.GroupBy(x => x.EventType);

                foreach (var group in groupedEvents)
                {
                    string eventType = group.Key;
                    List<string> ids = group.Select(x => x.Id).ToList();

                    switch (eventType)
                    {
                        case "products:updated":
                            var products = await _pimApiService.GetProductDataAsync(ids);
                            await _productWriteRepository.CacheUpdates(products);
                            break;
                        case "variants:updated":
                            var variants = await _pimApiService.GetVariantDataAsync(ids);
                            await _variantWriteRepository.CacheUpdates(variants);
                            break;
                        default:
                            break;
                    }

                    await _queueWriteRepository.RemoveFromQueueAsync(ids);
                }
            }
        } 
            
    }
}
