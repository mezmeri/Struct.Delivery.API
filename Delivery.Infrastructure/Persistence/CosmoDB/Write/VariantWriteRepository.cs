using Delivery.Application.Interfaces.Repositories;
using Delivery.Domain.DTO;
using Struct.App.Api.Models.Variant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Infrastructure.Persistence.CosmoDB.Write
{
    public class VariantWriteRepository : IVariantWriteRepository
    {
        public Task AddToCacheAsync(IEnumerable<VariantWithAttributesDTO> variants)
        {
            throw new NotImplementedException();
        }

        public Task DeleteFromCacheAsync(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        public Task UpdateToCacheAsync(IEnumerable<VariantWithAttributesDTO> variants)
        {
            throw new NotImplementedException();
        }
    }
}
