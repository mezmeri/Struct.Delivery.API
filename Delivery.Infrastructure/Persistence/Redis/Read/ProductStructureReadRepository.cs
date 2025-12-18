using Delivery.Domain.DTO;
using Struct.App.Api.Client;
using Struct.App.Api.Models.Product;
using Delivery.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Struct.App.Api.Models.ProductStructure;

namespace Delivery.Infrastructure.Persistence.Redis.Read
{
    public class ProductStructureReadRepository : IProductStructureReadRepository
    {
        private readonly StructApiClient _apiClient;

        public ProductStructureReadRepository(StructApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<ProductStructureWithAttributesDTO>> GetPimData(List<Guid> productStructureIds)
        {
            List<ProductStructure> basicModels = await GetBasicModel(productStructureIds);

            List<ProductStructureWithAttributesDTO> result = basicModels.Select(ps => new ProductStructureWithAttributesDTO
            {
                ProductStructure = ps
            }).ToList();

            return result;
        }


        private async Task<List<ProductStructure>> GetBasicModel(List<Guid> productStructureIds)
        {
            List<ProductStructure> productStructures = new List<ProductStructure>();

            foreach (var productStructureId in productStructureIds)
            {
                var productStructure = await _apiClient.ProductStructures.GetProductStructureAsync(productStructureId);

                if (productStructure != null)
                {
                    productStructures.Add(productStructure);
                }
            }

            return productStructures;
        }

    }
}
