using Delivery.Domain.DTO;
using Struct.App.Api.Models.Variant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Application.Interfaces.Repositories
{
    public interface IVariantWriteRepository
    {
        Task AddToCacheAsync (IEnumerable<VariantWithAttributesDTO> variants);
        Task UpdateToCacheAsync(IEnumerable<VariantWithAttributesDTO> variants);
        Task DeleteFromCacheAsync(IEnumerable<string> ids);
    }
}
