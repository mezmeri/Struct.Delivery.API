using Delivery.Application.Interfaces.Repositories;
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

        public async Task<IEnumerable<string>> FilterDirtyIds(IEnumerable<(string Id, long Timestamp, string)> idsWithTimestamps)
        {
            IEnumerable<(string, long, string)> queuedItems = await _queueReadRepository.GetQueueUpdates(idsWithTimestamps.Count());

            Dictionary<string, long> latestQueueTimestamps = queuedItems.ToDictionary(x => x.Item1, x => x.Item2);

            IEnumerable<string> cleanIds = idsWithTimestamps.Where(x => !latestQueueTimestamps.TryGetValue(x.Id, out var latest) || x.Timestamp >= latest)
                .Select(x => x.Id);

            return cleanIds;
        }
    }
}