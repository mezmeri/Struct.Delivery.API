using Delivery.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Application.Interfaces.Repositories
{
    public interface IQueueWriteRepository
    {
        //Task AddToQueueAsync(IEnumerable<string> ids);
        Task AddToQueueAsync(IEnumerable<ProductChangeQueueItem> changes);
        Task RemoveFromQueueAsync(IEnumerable<string> ids);
        Task RequeueIdsAsync(IEnumerable<string> ids);
    }
}
