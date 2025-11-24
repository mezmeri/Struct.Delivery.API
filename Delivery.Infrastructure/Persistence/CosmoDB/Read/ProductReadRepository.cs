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

        public Task<List<ProductModel>> GetPimData(List<int> ids)
        {
            throw new NotImplementedException();
        }
    }
}
