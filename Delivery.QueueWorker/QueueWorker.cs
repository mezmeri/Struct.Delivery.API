using Delivery.Application.Interfaces.Notifier;
using Delivery.Application.Interfaces.Repositories;
using Delivery.Application.Services;
using Delivery.Domain.Enum;
using Delivery.Domain.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.QueueWorker
{
    public class QueueWorker
    {
        private readonly IQueueReadRepository _queueReadRepository;
        private readonly IQueueWriteRepository _queueWriteRepository;
        private readonly IProductWriteRepository _productWriteRepository;
        private readonly FilterDirtyIdsService _filterDirtyIdsService;
        private readonly PimApiService _pimApiService;
        private readonly ILogger<QueueWorker> _logger;
        private readonly INotifier _notifier;

        public QueueWorker(IQueueReadRepository queueReadRepository, IProductWriteRepository productWriteRepository, FilterDirtyIdsService filterDirtyIdsService, PimApiService pimApiService, IQueueWriteRepository queueWriteRepository, ILogger<QueueWorker> logger, INotifier notifier)
        {
            _queueReadRepository = queueReadRepository;
            _queueWriteRepository = queueWriteRepository;
            _productWriteRepository = productWriteRepository;
            _filterDirtyIdsService = filterDirtyIdsService;
            _pimApiService = pimApiService;
            _logger = logger;
            _notifier = notifier;

        }

        public async Task ProcessQueueAsync()
        {
            IEnumerable<QueueItemDTO> queuedChanges = await _queueReadRepository.GetQueueUpdates(100);

            _logger.LogInformation($"Processing queue batch of {queuedChanges.Count()} items");

            if (!queuedChanges.Any())
            {
                return;
            }

            IEnumerable<string> cleanIds = await _filterDirtyIdsService.FilterDirtyIds(queuedChanges);

            List<QueueItemDTO> dirtyItems = queuedChanges.Where(x => !cleanIds.Contains(x.Id)).ToList();

            List<QueueItemDTO> cleanItems = queuedChanges.Where(x => cleanIds.Contains(x.Id)).ToList();

            _logger.LogInformation($"Clean items: {cleanItems.Count()}, Dirty items: {dirtyItems.Count()}");

            if (dirtyItems.Any())
            {
                await _queueWriteRepository.RequeueItemsAsync(dirtyItems);
            }

            if (cleanIds.Any())
            {
                var groupedEvents = cleanItems.GroupBy(x => new { x.EventType, x.EntityType });

                foreach (var group in groupedEvents)
                {
                    string eventType = group.Key.EventType;
                    List<QueueItemDTO> itemsInGroup = group.ToList();
                    List<string> ids = group.Select(x => x.Id).ToList();
                    EntityType entityType = group.Key.EntityType;


                    _logger.LogInformation($"Processing event type {eventType} with {ids.Count()} items");

                    switch (eventType)
                    {
                        case "products:updated":
                            var products = await _pimApiService.GetProductDataAsync(ids);
                            await _productWriteRepository.CacheUpdates(products);
                            break;
                        default:
                            _logger.LogInformation($"No repository for event type {eventType}");
                            break;
                    }

                    await _queueWriteRepository.RemoveFromQueueAsync(itemsInGroup);

                    await _notifier.NotifyChangesAsync(ids, eventType, DateTimeOffset.UtcNow, entityType.ToString());
                }
            }
        }
    }
}

