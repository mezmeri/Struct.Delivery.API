using Delivery.Application.Interfaces.Repositories;
using Delivery.Application.Services;
using Delivery.Domain.DTO;
using Newtonsoft.Json;
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
        private readonly IDatabase _database;
        private const string _cacheHashKey = "products:cached";

        public ProductReadRepository(StructApiClient apiClient, IConnectionMultiplexer redis)
        {
            _apiClient = apiClient;
            _database = redis.GetDatabase();
        }

        public async Task<List<ProductWithAttributesDTO>> GetPimData(List<int> productIds)
        {
            List<ProductModel> basicModels = await GetBasicModel(productIds);
            List<ProductAttributeValuesModel<Dictionary<string, object>>> attributeValues = await GetProductValues<Dictionary<string, object>>(productIds);

            Dictionary<int, Dictionary<string, object>> valuesDict = attributeValues.ToDictionary(v => v.ProductId, v => v.Values);

            List<ProductWithAttributesDTO> result = basicModels.Select(p => new ProductWithAttributesDTO
            {
                Product = p,
                AttributeValues = valuesDict.ContainsKey(p.Id) ? valuesDict[p.Id] : null
            }).ToList();

            return result;  
        }

        //Retrieve single product fra cachen
        public async Task<ProductWithAttributesDTO?> GetCachedProductAsync(string productId)
        {
            RedisValue value = await _database.HashGetAsync(_cacheHashKey, productId);

            if (value.IsNullOrEmpty)
                return null;

            return JsonConvert.DeserializeObject<ProductWithAttributesDTO>(value);
        }

        //Retrieve all products fra cachen - Returnere STORT dataset!
        public async Task<List<ProductWithAttributesDTO>> GetAllCachedProductsAsync()
        {
            HashEntry[] entries = await _database.HashGetAllAsync(_cacheHashKey);

            return entries
                .Select(e => JsonConvert.DeserializeObject<ProductWithAttributesDTO>(e.Value))
                .Where(p => p != null)
                .ToList();
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
