using Delivery.Application.Interfaces.Repositories;
using Delivery.Domain.DTO;
using Struct.App.Api.Client;
using Struct.App.Api.Models.Product;
using Struct.App.Api.Models.VariantGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Infrastructure.Persistence.Redis.Read
{
    public class VariantGroupReadRepository : IVariantGroupReadRepository
    {
        private readonly StructApiClient _apiClient;

        public VariantGroupReadRepository(StructApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<VariantGroupWithAttributesDTO>> GetPimData(List<int> variantGroupIds)
        {
            List<VariantGroupModel> basicModels = await GetBasicModel(variantGroupIds);
            List<VariantGroupAttributeValuesModel<Dictionary<string, object>>> attributeValues = await GetVariantGroupValues<Dictionary<string, object>>(variantGroupIds);

            Dictionary<int, Dictionary<string, object>> valuesDict = attributeValues.ToDictionary(v => v.VariantGroupId, v => v.Values);

            List<VariantGroupWithAttributesDTO> result = basicModels.Select(v => new VariantGroupWithAttributesDTO
            {
                VariantGroup = v,
                AttributeValues = valuesDict.ContainsKey(v.Id) ? valuesDict[v.Id] : null
            }).ToList();

            return result;
        }


        private async Task<List<VariantGroupModel>> GetBasicModel(List<int> variantGroupIds)
        {
            return await _apiClient.VariantGroups.GetVariantGroupsAsync(variantGroupIds);
        }

        private async Task<List<VariantGroupAttributeValuesModel<T>>> GetVariantGroupValues<T>(List<int> variantGroupIds)
        {
            VariantGroupValuesRequestModel variantGroupValuesRequestModel = new()
            {
                VariantGroupIds = variantGroupIds
            };

            return await _apiClient.VariantGroups.GetVariantGroupAttributeValuesAsync<T>(variantGroupValuesRequestModel);
        }
    }
}
