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
        Task AddToQueueAsync(IEnumerable<string> ids);
        // Bruges til at tilføje hele objekter med anden info en blot ID
        Task AddEntityUpdatesToQueueAsync(IEnumerable<EntityItem> changes);
        Task RemoveFromQueueAsync(IEnumerable<string> ids);
        Task RequeueIdsAsync(IEnumerable<string> ids);
    }
}
