using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Application.Services
{
    public class QueueService
    {
        private readonly ILogger<QueueService> _logger;
        public QueueService(ILogger<QueueService> logger)
        {
            _logger = logger;
        }
    }
}
