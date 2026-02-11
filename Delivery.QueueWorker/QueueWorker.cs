using Delivery.Application.Interfaces.Notifier;
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
using Delivery.Application.Interfaces;

namespace Delivery.QueueWorker
{
    public class QueueWorker : BackgroundService
    {
        private readonly IQueueReadRepository _queueReadRepository;
        private readonly IQueueWriteRepository _queueWriteRepository;
        private readonly FilterDirtyIdsService _filterDirtyIdsService;
        private readonly ILogger<QueueWorker> _logger;
        private readonly INotifier _notifier;
        private readonly IReadOnlyDictionary<EntityType, IEntityEventService> _entityEvents;

        public QueueWorker(IQueueReadRepository queueReadRepository, FilterDirtyIdsService filterDirtyIdsService, IQueueWriteRepository queueWriteRepository, ILogger<QueueWorker> logger, INotifier notifier, IEnumerable<IEntityEventService> entityEvents)
        {
            _queueReadRepository = queueReadRepository;
            _queueWriteRepository = queueWriteRepository;
            _filterDirtyIdsService = filterDirtyIdsService;
            _logger = logger;
            _notifier = notifier;
            _entityEvents = entityEvents.ToDictionary(s => s.EntityType);

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

                    if(_entityEvents.TryGetValue(entityType, out var entityEvent))
                    {
                        await entityEvent.ProcessAsync(eventType, ids);
                    }
                    else
                    {
                        _logger.LogInformation($"No repository for event type {entityType}");
                    }

                    await _queueWriteRepository.RemoveFromQueueAsync(group);

                    await _notifier.NotifyChangesAsync(
                        ids,
                        eventType,
                        DateTimeOffset.UtcNow,
                        entityType.ToString());
                }
            }

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
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}