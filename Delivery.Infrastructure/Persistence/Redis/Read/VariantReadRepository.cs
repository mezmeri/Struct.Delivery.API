using Delivery.Application.Interfaces.Repositories;
using Delivery.Domain.DTO;
using NPOI.HSSF.EventUserModel;
using Struct.App.Api.Client;
using Struct.App.Api.Models.Product;
using Struct.App.Api.Models.Variant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Infrastructure.Persistence.Redis.Read
{
    public class VariantReadRepository : IVariantReadRepository
    {
        private readonly StructApiClient _apiClient;

        public VariantReadRepository(StructApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<VariantWithAttributesDTO>> GetPimData(List<int> variantIds)
        {
            List<VariantModel> basicModels = await GetBasicModel(variantIds);
            List<VariantAttributeValuesModel<Dictionary<string, object>>> attributeValues = await GetVariantValues<Dictionary<string, object>>(variantIds);

            Dictionary<int, Dictionary<string, object>> valuesDict = attributeValues.ToDictionary(v => v.VariantId, v => v.Values);

            List<VariantWithAttributesDTO> result = basicModels.Select(v => new VariantWithAttributesDTO
            {
                Variant = v,
                AttributeValues = valuesDict.ContainsKey(v.Id) ? valuesDict[v.Id] : null
            }).ToList();

            return result;
        }

        private async Task<List<VariantModel>> GetBasicModel(List<int> variantIds)
        {
            return await _apiClient.Variants.GetVariantsAsync(variantIds);
        }

        private async Task<List<VariantAttributeValuesModel<T>>> GetVariantValues<T>(List<int> variantIds)
        {
            VariantValuesRequestModel variantValuesRequestModel = new()
            {
                VariantIds = variantIds
            };

            return await _apiClient.Variants.GetVariantAttributeValuesAsync<T>(variantValuesRequestModel);
        }
    }
}
