using Delivery.Application.Interfaces.Repositories;
using Struct.App.Api.Models.Variant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Infrastructure.Persistence.CosmoDB.Read
{
    public class VariantReadRepository : IVariantReadRepository
    {
        public Task<List<VariantModel>> GetPimData(List<int> ids)
        {
            throw new NotImplementedException();
        }
    }
}
