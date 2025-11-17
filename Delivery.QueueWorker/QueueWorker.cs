using Delivery.Application.Interfaces.Repositories;
using Delivery.Application.Services;

namespace Delivery.QueueWorker
{
    public class QueueWorker
    {
        IProductWriteRepository _productWriteRepository;
        IProductReadRepository _productReadRepository;
        IQueueReadRepository _queueReadRepository;
        FilterDirtyIdsService _filterDirtyIdsService;

        public QueueWorker(IProductWriteRepository productWriteRepository, IProductReadRepository productReadRepository, IQueueReadRepository queueReadRepository, FilterDirtyIdsService filterDirtyIdsService)
        {
            _productWriteRepository = productWriteRepository;
            _productReadRepository = productReadRepository;
            _queueReadRepository = queueReadRepository;
            _filterDirtyIdsService = filterDirtyIdsService;
        }

        public async Task ProcessQueueAsync()
        {
            var queuedItems = await _queueReadRepository.GetProductChanges(100);

            if (!queuedChanges.Any())
            {
                return;
            }

            IEnumerable<string> cleanIds = await _filterDirtyIdsService.FilterDirtyIds(queuedChanges);

            if (!cleanIds.Any())
            {
                return;
            }

            var data = await _productReadRepository.GetPimData(cleanIds);

            await _productWriteRepository.CacheUpdates(data);

            await _queueReadRepository.RemoveFromQueueAsync(cleanIds);

        }
    }
}
