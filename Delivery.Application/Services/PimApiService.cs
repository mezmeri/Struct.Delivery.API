using Struct.App.Api.Client;
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
        private readonly StructApiClient _apiClient;

        public PimApiService(StructApiClient apiClient)
        {
            _apiClient = apiClient;
        }
        public async Task<IEnumerable<ProductModel>> GetProductDataAsync(List<int> productIds)
        {
            return await _apiClient.Products.GetProductsAsync(productIds);
        }
    }
}
