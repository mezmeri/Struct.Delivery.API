using Delivery.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Application.Interfaces.Repositories
{
    public interface IQueueReadRepository
    {
        Task<IEnumerable<QueueItemDTO>> GetQueueUpdates(int batchSize);
        Task<Dictionary<string, long>> GetLatestTimestampsAsync(IEnumerable<string> ids);

    }
}
