using Delivery.Application.Interfaces.Repositories;
using Delivery.Domain.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Application.Services
{
    public class FilterDirtyIdsService
    {
        private readonly IQueueReadRepository _queueReadRepository;
        private readonly ILogger<FilterDirtyIdsService> _logger;

        public FilterDirtyIdsService(IQueueReadRepository queueReadRepository, ILogger<FilterDirtyIdsService> logger)
        {
            _queueReadRepository = queueReadRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<string>> FilterDirtyIds(IEnumerable<QueueItemDTO> items)
        {
            List<string> ids = items.Select(x => x.Id).Distinct().ToList();

            Dictionary<string, long> latestTimestamps = await _queueReadRepository.GetLatestTimestampsAsync(ids);

            IEnumerable<string> cleanIds = items.Where(x => !latestTimestamps.TryGetValue(x.Id, out var latest) || x.Timestamp >= latest)
                .Select(x => x.Id).Distinct().ToList();

            _logger.LogInformation($"Filtered {cleanIds.Count()} clean and {ids.Count() - cleanIds.Count()} dirty items");

            return cleanIds;
        }
    }
}