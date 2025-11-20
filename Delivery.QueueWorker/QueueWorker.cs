using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// HUSK HUSK!!! Add "Project Reference v højreklik af "Delivery.QueueWorker"
using Delivery.Application.Interfaces.Repositories;
using Delivery.Application.Services;

namespace Delivery.QueueWorker
{
    public class QueueWorker
    {

        private readonly IQueueReadRepository _queueReadRepository;
        private readonly IProductWriteRepository _productWriteRepository;
        private readonly FilterDirtyIdsService _filterDirtyIdsService;
        private readonly PimApiService _pimApiService;


        public QueueWorker(IQueueReadRepository queueReadRepository, IProductWriteRepository productWriteRepository, FilterDirtyIdsService filterDirtyIdsService, PimApiService pimApiService)
        
        {
            _queueReadRepository = queueReadRepository;
            _productWriteRepository = productWriteRepository;
            _filterDirtyIdsService = filterDirtyIdsService;
            _pimApiService = pimApiService;

            
        }


        public async Task ProcessQueueAsync() { 
        } //TO BE IMPLEMENTED
    }
}
