using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Application.Interfaces.Repositories
{
    public interface IQueueWriteRepository
    {
        Task AddToQueueAsync(string eventType, IEnumerable<string> ids);
        Task RemoveFromQueueAsync(string eventType, IEnumerable<string> ids);
        Task RequeueIdsAsync(string eventType, IEnumerable<string> ids);
    }
}
