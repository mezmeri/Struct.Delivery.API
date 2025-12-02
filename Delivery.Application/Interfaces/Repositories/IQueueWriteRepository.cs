using Delivery.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Application.Interfaces.Repositories
{
    public interface IQueueWriteRepository
    {
        Task AddToQueueAsync(IEnumerable<QueueItemEventArgs> events);
        Task RemoveFromQueueAsync(IEnumerable<string> ids);
        Task RequeueItemsAsync(IEnumerable<QueueItemEventArgs> items);
    }
}
