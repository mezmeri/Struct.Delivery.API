using Delivery.Application.Interfaces.Repositories;
using Delivery.Application.Services;
using Delivery.Domain.Enum;
using Delivery.Domain.Events;
using Microsoft.Extensions.Logging;
using Struct.App.Api.Models.Product;
using Struct.App.Api.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Delivery.Application.Interfaces.Repositories;
using Delivery.Application.Services;
using Microsoft.Extensions.Hosting;
using Delivery.Domain.DTO;

namespace Delivery.QueueWorker
{
    public class QueueWorker : BackgroundService
    {
        private readonly IQueueReadRepository _queueReadRepository;
        private readonly IQueueWriteRepository _queueWriteRepository;
        private readonly IProductWriteRepository _productWriteRepository;
        private readonly IVariantWriteRepository _variantWriteRepository;
        private readonly FilterDirtyIdsService _filterDirtyIdsService;
        private readonly PimApiService _pimApiService;
        private readonly ILogger<QueueWorker> _logger;

        public QueueWorker(IQueueReadRepository queueReadRepository, IProductWriteRepository productWriteRepository, IVariantWriteRepository variantWriteRepository, FilterDirtyIdsService filterDirtyIdsService, PimApiService pimApiService, IQueueWriteRepository queueWriteRepository, ILogger<QueueWorker> logger)
        {
            _queueReadRepository = queueReadRepository;
            _queueWriteRepository = queueWriteRepository;
            _productWriteRepository = productWriteRepository;
            _variantWriteRepository = variantWriteRepository;
            _filterDirtyIdsService = filterDirtyIdsService;
            _pimApiService = pimApiService;
            _logger = logger;

        }

        public async Task ProcessQueueAsync()
        {
            IEnumerable<QueueItemDTO> queuedChanges = await _queueReadRepository.GetQueueUpdates(100);

            if (!queuedChanges.Any())
            {
                return;
            }

            _logger.LogInformation($"Processing queue batch of {queuedChanges.Count()} items");

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
                            List<ProductWithAttributesDTO> products = new();
                            
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
                        
                        
                        case EntityType.Variant:
                            List<VariantWithAttributesDTO> variants = new();

                            if(eventType != "variants:deleted")
                            {
                                variants = (await _pimApiService.GetVariantDataAsync(ids)).ToList();
                            }
                            if (eventType == "variants:created")
                            {
                                await _variantWriteRepository.AddToCacheAsync(variants);
                            }
                            else if (eventType == "variants:updated")
                            {
                                await _variantWriteRepository.UpdateToCacheAsync(variants);
                            }
                            else if (eventType == "variants:deleted")
                            {
                                await _variantWriteRepository.DeleteFromCacheAsync(ids);
                            }
                            _logger.LogInformation($"Variant event processing not implemented yet for event type {eventType}");
                            break;

                        default:
                            _logger.LogInformation($"No repository for event type {eventType}");
                            break;
                    }

                    await _queueWriteRepository.RemoveFromQueueAsync(group);
                }
            }
            //    await _queueReadRepository.RemoveFromQueueAsync(cleanIds);
            //}
        }

        /// <summary>
        /// Inherited from the BackgroundService class, which are a part of the whole Host environment setup in Program.cs. We might have to use this?
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                { 
                    await ProcessQueueAsync(); 
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing queue");
                }
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}