using Delivery.Application.Interfaces.Repositories;
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
        public Task CacheUpdates(IEnumerable<VariantModel> variants)
        {
            throw new NotImplementedException();
        }
    }
}
