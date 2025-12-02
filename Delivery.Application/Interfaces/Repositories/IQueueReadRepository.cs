using Delivery.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Application.Interfaces.Repositories
{
    public interface IQueueReadRepository
    {
        //Task<IEnumerable<(string, long)>> GetProductChanges(int batchSize);
        Task<IEnumerable<ProductChangeQueueItem>> GetProductChanges(int batchSize);

    }
}
