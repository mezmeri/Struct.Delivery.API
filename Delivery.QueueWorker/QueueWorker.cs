using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Delivery.Application.Interfaces.Repositories;
using Delivery.Application.Services;
using Microsoft.Extensions.Hosting;

namespace Delivery.QueueWorker
{
    public class QueueWorker : BackgroundService
    {

        private readonly IQueueReadRepository _queueReadRepository;
        private readonly IProductWriteRepository _productWriteRepository;
        private readonly FilterDirtyIdsService _filterDirtyIdsService;
        private readonly PimApiService _pimApiService;


        public QueueWorker(IQueueReadRepository queueReadRepository, IProductWriteRepository productWriteRepository, FilterDirtyIdsService filterDirtyIdsService, PimApiService pimApiService)

        {
            _queueReadRepository = queueReadRepository;
            _productWriteRepository = productWriteRepository;
            _filterDirtyIdsService = filterDirtyIdsService;
            _pimApiService = pimApiService;


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


            //if (dirtyIds.Any())
            //{
            //    await _queueReadRepository.(dirtyIds);
            //}

            //if (cleanIds.Any())
            //{
            //    var data = await _pimApiService.GetProductDataAsync(cleanIds);

            //    await _productWriteRepository.CacheUpdates(data);

            //    await _queueReadRepository.RemoveFromQueueAsync(cleanIds);
            //}
        }

        /// <summary>
        /// Inherited from the BackgroundService class, which are a part of the whole Host environment setup in Program.cs. We might have to use this?
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
