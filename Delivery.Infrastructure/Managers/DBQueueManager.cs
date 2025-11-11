using Delivery.Application.Interfaces.Managers;
using Delivery.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Infrastructure.Managers
{
    public class DBQueueManager : IDbQueueManager
    {
        IProductWriteRepository _productWriterRepository;
        public DBQueueManager(IProductWriteRepository productWriteRepository) 
        {
            _productWriterRepository = productWriteRepository;
        }

        public Task EnqueueUpdatesAsync(IEnumerable<string> ids)
        {
           return _productWriterRepository.AddToQueueAsync(ids);
        }

    }
}
