using Delivery.Application.Interfaces.Managers;
using Delivery.Application.Interfaces.Repositories;
using Delivery.Application.Models;
using Delivery.Application.Services;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger <QueueManager> _logger;

        public QueueManager(IQueueWriteRepository queueWriteRepository, IQueueReadRepository queueReadRepository,IProductWriteRepository productWriteRepository, IProductReadRepository productReadRepository, FilterDirtyIdsService filterDirtyIdsService, PimApiService pimApiService, ILogger<QueueManager> logger) 
        {
            _queueWriteRepository = queueWriteRepository;
            _queueReadRepository = queueReadRepository;
            _productWriteRepository = productWriteRepository;
            _productReadRepository = productReadRepository;
            _filterDirtyIdsService = filterDirtyIdsService;
            _pimApiService = pimApiService;
            _logger = logger;
        }

        public async Task EnqueueEntityUpdatesAsync(IEnumerable<EntityItem> changes)
        {
            await _queueWriteRepository.AddEntityUpdatesToQueueAsync(changes);

            await Task.Delay(9000);

            await ProcessEntityUpdateQueueAsync();
        }

        public async Task ProcessEntityUpdateQueueAsync()
        {

            IEnumerable<EntityItem> queuedChanges = await _queueReadRepository.GetEntityUpdateChanges(100);
            if (!queuedChanges.Any())
            {
                return;
            }

            // Added variable for evt debugging eller logging.
            var queuedTuples = queuedChanges.Select(x => (x.Id, x.Timestamp));
            IEnumerable<string> cleanIds = await _filterDirtyIdsService.FilterDirtyIds(queuedTuples);

            List<string> dirtyIds = queuedChanges.Select(x => x.Id).Except(cleanIds).ToList();

            if (dirtyIds.Any())
            {
                await _queueWriteRepository.RequeueIdsAsync(dirtyIds);
            }

            if (cleanIds.Any())
            {
                var data = await _pimApiService.GetEntityDataAsync(cleanIds);

                await _productWriteRepository.CacheUpdates(data);

                await _queueWriteRepository.RemoveFromQueueAsync(cleanIds);
            }

        }
    }
}
