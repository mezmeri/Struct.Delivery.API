using Delivery.Application.Interfaces.Repositories;
using Struct.App.Api.Models.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Infrastructure.Persistence.CosmoDB.Write
{
    public class ProductWriteRepository : IProductWriteRepository
    {
        public ProductWriteRepository() { }

        public Task CacheUpdates(IEnumerable<ProductModel> products)
        {
            throw new NotImplementedException();
        }
    }
}
