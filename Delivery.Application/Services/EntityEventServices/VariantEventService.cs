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
    public class VariantEventService : IEntityEventService
    {
        public EntityType EntityType => EntityType.Variant;

        private readonly PimApiService _pimApiService;
        private readonly IVariantWriteRepository _variantWriteRepository;

        public VariantEventService(PimApiService pimApiService,IVariantWriteRepository variantWriteRepository)
        {
            _pimApiService = pimApiService;
            _variantWriteRepository = variantWriteRepository;
        }

        public async Task ProcessAsync(string eventType, List<string> ids)
        {
            switch (eventType)
            {
                case "variants:created":
                    List<VariantWithAttributesDTO> createdVariants = (await _pimApiService.GetVariantDataAsync(ids)).ToList();
                    await _variantWriteRepository.AddToCacheAsync(createdVariants);
                    break;

                case "variants:updated":
                    List<VariantWithAttributesDTO> updatedVariants = (await _pimApiService.GetVariantDataAsync(ids)).ToList();
                    await _variantWriteRepository.UpdateToCacheAsync(updatedVariants);
                    break;

                case "variants:deleted":
                    await _variantWriteRepository.DeleteFromCacheAsync(ids);
                    break;
            }
        }
    }

}
