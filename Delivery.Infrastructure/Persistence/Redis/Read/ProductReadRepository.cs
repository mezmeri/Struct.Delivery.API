using Delivery.Application.Interfaces.Repositories;
using Delivery.Application.Services;
using StackExchange.Redis;
using Struct.App.Api.Client;
using Struct.App.Api.Models;
using Struct.App.Api.Models.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Delivery.Infrastructure.Persistence.Redis.Read
{
    public class ProductReadRepository : IProductReadRepository
    {
        private readonly StructApiClient _apiClient;

        public ProductReadRepository(StructApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<ProductModel>> GetPimData(List<int> productIds)
        {
            return await _apiClient.Products.GetProductsAsync(productIds);
        }
    }
}
