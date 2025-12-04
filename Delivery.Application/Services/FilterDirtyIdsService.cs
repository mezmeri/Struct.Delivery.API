using Delivery.Application.Interfaces.Repositories;
using Delivery.Application.Models;
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
        public FilterDirtyIdsService(IQueueReadRepository queueReadRepository)
        {
            _queueReadRepository = queueReadRepository;
        }

        public async Task<IEnumerable<string>> FilterDirtyIds(IEnumerable<(string Id, long Timestamp)> idsWithTimestamps)
        {
            IEnumerable<EntityItem> queuedItems = await _queueReadRepository.GetEntityUpdateChanges(idsWithTimestamps.Count());

            Dictionary<string, long> latestQueueTimestamps = queuedItems.ToDictionary(x => x.Id, x => x.Timestamp);

            IEnumerable<string> cleanIds = idsWithTimestamps.Where(x => !latestQueueTimestamps.TryGetValue(x.Id, out var latest) || x.Timestamp >= latest)
                .Select(x => x.Id);
            return cleanIds;
        }
    }
}