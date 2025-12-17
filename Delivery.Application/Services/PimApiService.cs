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
        private readonly IVariantReadRepository _variantReadRepository;

        public PimApiService(IProductReadRepository productReadRepository, IVariantReadRepository variantReadRepository)
        {
            _productReadRepository = productReadRepository;
            _variantReadRepository = variantReadRepository;
        }
        public async Task<List<ProductWithAttributesDTO>> GetProductDataAsync(IEnumerable<string> ids)
        {
            List<int> productIds = ids.Select(int.Parse).ToList();

            return await _productReadRepository.GetPimData(productIds);

        }

        public async Task<List<VariantWithAttributesDTO>> GetVariantDataAsync(IEnumerable<string> ids)
        {
            List<int> variantIds = ids.Select(int.Parse).ToList();
            return await _variantReadRepository.GetPimData(variantIds);
        }
    }
}
