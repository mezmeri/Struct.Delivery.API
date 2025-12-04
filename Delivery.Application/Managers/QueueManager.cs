using Delivery.Application.Interfaces.Managers;
using Delivery.Application.Interfaces.Repositories;
using Delivery.Application.Services;
using Delivery.Domain.Events;
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

        public async Task EnqueueUpdatesAsync(IEnumerable<QueueItemDTO> events)
        {
            await _queueWriteRepository.AddToQueueAsync(ids);   
        }
    }
}
