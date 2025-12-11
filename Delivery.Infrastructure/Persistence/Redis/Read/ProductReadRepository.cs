using Delivery.Application.Interfaces.Repositories;
using Delivery.Application.Services;
using Delivery.Domain.DTO;
using NPOI.SS.Formula.Functions;
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

        public async Task<List<ProductWithAttributesDTO>> GetPimData(List<int> productIds)
        {
            var basicModels = await GetBasicModel(productIds);
            var attributeValues = await GetProductValues<T>(productIds);

            var valuesDict = attributeValues.ToDictionary(v => v.ProductId, v => v.Values);

            var result = basicModels.Select(p => new ProductWithAttributesDTO
            {
                Product = p,
                AttributeValues = valuesDict.ContainsKey(p.Id) ? valuesDict[p.Id] : null
            }).ToList();

            return result;  
        }


        private async Task<List<ProductModel>> GetBasicModel(List<int> productIds)
        {
            return await _apiClient.Products.GetProductsAsync(productIds);
        }

        private async Task<List<ProductAttributeValuesModel<T>>> GetProductValues<T>(List<int> productIds)
        {
            ProductValuesRequestModel productValuesRequestModel = new()
            {
                ProductIds = productIds
            };

            return await _apiClient.Products.GetProductAttributeValuesAsync<T>(productValuesRequestModel);
        }
    }
}
