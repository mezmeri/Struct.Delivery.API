using Delivery.Application.Interfaces.Repositories;
using Delivery.Application.Services;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Struct.App.Api.Models;
using Struct.App.Api.Models.Product;
using System.Text.Json;

namespace Delivery.Infrastructure.Persistence.Redis.Read
{
    public class ProductReadRepository : IProductReadRepository
    {
        private PimApiService _pimApiService;

        public ProductReadRepository(PimApiService pimApiService)
        {
            _pimApiService = pimApiService;
        }

        public async Task<IEnumerable<ProductModel>> GetPimData(IEnumerable<string> ids)
        {
            var productIds = ids.Select(id => int.TryParse(id, out var result) ? (int?)result : null).OfType<int>().ToList();

            return await _pimApiService.GetProductDataAsync(productIds);

        }
    }
}
