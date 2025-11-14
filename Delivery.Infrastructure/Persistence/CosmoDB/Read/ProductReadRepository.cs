using Delivery.Application.Interfaces.Repositories;
using Struct.App.Api.Models.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Infrastructure.Persistence.CosmoDB.Read
{
    public class ProductReadRepository : IProductReadRepository
    {
        public Task CacheUpdates(IEnumerable<ProductModel> products)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ProductModel>> GetPimData(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<(string, long)>> GetProductChanges()
        {
            throw new NotImplementedException();
        }

        public Task RemoveFromQueueAsync(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }
    }
}
