using Delivery.Application.Interfaces.Repositories;
using Delivery.Domain.DTO;
using Struct.App.Api.Models.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Application.Services
{
    public class PimApiService
    {
        private readonly IProductReadRepository _productReadRepository;
        private readonly IProductStructureReadRepository _structureReadRepository;

        public PimApiService(IProductReadRepository productReadRepository, IProductStructureReadRepository structureReadRepository)
        {
            _productReadRepository = productReadRepository;
            _structureReadRepository = structureReadRepository;
        }
        public async Task<List<ProductWithAttributesDTO>> GetProductDataAsync(IEnumerable<string> ids)
        {
            List<int> productIds = ids.Select(int.Parse).ToList();

            return await _productReadRepository.GetPimData(productIds);

        }
        public async Task<List<ProductStructureWithAttributesDTO>> GetProductStructureDataAsync(IEnumerable<string> ids)
        {
            List<Guid> productIds = ids.Select(Guid.Parse).ToList();

            return await _structureReadRepository.GetPimData(productIds);

        }
    }
}
