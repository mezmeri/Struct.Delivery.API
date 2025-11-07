using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Application.Interfaces
{
    public interface IProductWriteRepository
    {
        Task AddToQueueAsync(string id);
        Task AddToQueueAsync(IEnumerable<string> ids);
    }
}
