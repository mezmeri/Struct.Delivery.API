using Delivery.Application.Interfaces.Repositories;
using Delivery.Application.Services;
using Delivery.Domain.Enum;
using Delivery.Domain.Events;
using Microsoft.Extensions.Logging;
using Struct.App.Api.Models.Product;

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

        public QueueWorker(IQueueReadRepository queueReadRepository, IProductWriteRepository productWriteRepository, FilterDirtyIdsService filterDirtyIdsService, PimApiService pimApiService, IQueueWriteRepository queueWriteRepository, ILogger<QueueWorker> logger)
        {
            _queueReadRepository = queueReadRepository;
            _queueWriteRepository = queueWriteRepository;
            _productWriteRepository = productWriteRepository;
            _filterDirtyIdsService = filterDirtyIdsService;
            _pimApiService = pimApiService;
            _logger = logger;

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
                var groupedEvents = cleanItems.GroupBy(x => new { x.EntityType, x.EventType });

                foreach (var group in groupedEvents)
                {
                    string eventType = group.Key.EventType;
                    EntityType entityType = group.Key.EntityType;
                    List<string> ids = group.Select(x => x.Id).ToList();

                    _logger.LogInformation($"Processing event type {eventType} with {ids.Count()} items");

                    switch (entityType)
                    {
                        case EntityType.Product:
                            List<ProductModel> products = new();
                            
                            if (eventType != "products:deleted")
                            {
                                products = (await _pimApiService.GetProductDataAsync(ids)).ToList();
                            }

                            if (eventType == "products:created")
                                await _productWriteRepository.AddToCacheAsync(products);
                            else if (eventType == "products:updated")
                                await _productWriteRepository.UpdateToCacheAsync(products);
                            else if (eventType == "products:deleted")
                                await _productWriteRepository.DeleteFromCacheAsync(ids);
                            break;
                        default:
                            _logger.LogInformation($"No repository for event type {eventType}");
                            break;
                    }

                    await _queueWriteRepository.RemoveFromQueueAsync(group);
                }
            }
        }
    }
}

