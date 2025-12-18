using Delivery.Domain.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Application.Interfaces.Repositories
{
    public interface IProductStructureWriteRepository
    {
        Task AddToCacheAsync(IEnumerable<ProductStructureWithAttributesDTO> productStructures);
        Task UpdateToCacheAsync(IEnumerable<ProductStructureWithAttributesDTO> productStructures);
        Task DeleteFromCacheAsync(IEnumerable<string> ids);
    }
}
