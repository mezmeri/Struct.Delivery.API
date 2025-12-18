using Delivery.Domain.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Application.Interfaces.Repositories
{
    public interface IVariantGroupWriteRepository
    {
        Task AddToCacheAsync(IEnumerable<VariantGroupWithAttributesDTO> variantGroups);
        Task UpdateToCacheAsync(IEnumerable<VariantGroupWithAttributesDTO> variantGroups);
        Task DeleteFromCacheAsync(IEnumerable<string> ids);
    }
}
