using Delivery.Application.Interfaces.Managers;
using Delivery.Application.Interfaces.Repositories;
using Delivery.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Infrastructure.Managers
{
    public class QueueManager : IQueueManager
    {
        private readonly IQueueWriteRepository _queueWriteRepository;

        public QueueManager(IQueueWriteRepository queueWriteRepository)
        {
            _queueWriteRepository = queueWriteRepository;  
        }

        public async Task EnqueueUpdatesAsync(IEnumerable<string> ids)
        {
            await _queueWriteRepository.AddToQueueAsync(ids);   
        }
    }
}
