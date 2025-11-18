using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Delivery.Application.Interfaces.Repositories;
using Struct.App.Api.Models.Product;

namespace Delivery.Application.Services
{
    public class PimApiService
    {
        private readonly IProductReadRepository _productReadRepository;

        public PimApiService(IProductReadRepository productReadRepository)
        {
            _productReadRepository = productReadRepository;
        }
        public async Task<List<ProductModel>> GetProductDataAsync(IEnumerable<string> ids)
        {
            List<int> productIds = ids.Select(int.Parse).ToList();

            return await _productReadRepository.GetPimData(productIds);

        }
    }
}
