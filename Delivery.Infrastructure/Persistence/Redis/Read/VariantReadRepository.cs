using Delivery.Application.Interfaces.Repositories;
using Struct.App.Api.Models.Variant;
using System;
using System.Collections.Generic;
using Struct.App.Api.Models.Variant;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Struct.App.Api.Client;

namespace Delivery.Infrastructure.Persistence.Redis.Read
{
    public class VariantReadRepository : IVariantReadRepository
    {
        private readonly StructApiClient _apiClient;

        public VariantReadRepository(StructApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<VariantModel>> GetPimData(List<int> VariantIds)
        {
            return await _apiClient.Variants.GetVariantsAsync(VariantIds);
        }
    }
}
