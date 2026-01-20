using Delivery.Application.Interfaces;
using Delivery.Application.Interfaces.Repositories;
using Delivery.Domain.DTO;
using Delivery.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Application.Services.EntityEventServices
{
    public class ProductEventService : IEntityEventService
    {
        public EntityType EntityType => EntityType.Product;

        private readonly PimApiService _pimApiService;
        private readonly IProductWriteRepository _productWriteRepository;

        public ProductEventService(PimApiService pimApiService,IProductWriteRepository productWriteRepository)
        {
            _pimApiService = pimApiService;
            _productWriteRepository = productWriteRepository;
        }

        public async Task ProcessAsync(string eventType, List<string> ids)
        {
            switch (eventType)
            {
                case "products:created":
                    List<ProductWithAttributesDTO> createdProducts = (await _pimApiService.GetProductDataAsync(ids)).ToList();
                    await _productWriteRepository.AddToCacheAsync(createdProducts);
                    break;

                case "products:updated":
                    List<ProductWithAttributesDTO> updatedProducts = (await _pimApiService.GetProductDataAsync(ids)).ToList();
                    await _productWriteRepository.UpdateToCacheAsync(updatedProducts);
                    break;

                case "products:deleted":
                    await _productWriteRepository.DeleteFromCacheAsync(ids);
                    break;
            }
        }
    }

}
