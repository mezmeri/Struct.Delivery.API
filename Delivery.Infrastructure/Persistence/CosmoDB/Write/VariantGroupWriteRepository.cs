using Delivery.Application.Interfaces.Repositories;
using Delivery.Domain.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Infrastructure.Persistence.CosmoDB.Write
{
    public class VariantGroupWriteRepository : IVariantGroupWriteRepository
    {
        public Task AddToCacheAsync(IEnumerable<VariantGroupWithAttributesDTO> variantGroups)
        {
            throw new NotImplementedException();
        }

        public Task DeleteFromCacheAsync(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        public Task UpdateToCacheAsync(IEnumerable<VariantGroupWithAttributesDTO> variantGroups)
        {
            throw new NotImplementedException();
        }
    }
}
