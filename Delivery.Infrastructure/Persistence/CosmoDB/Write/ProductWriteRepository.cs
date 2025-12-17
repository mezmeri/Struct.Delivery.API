using Delivery.Application.Interfaces.Repositories;
using Delivery.Domain.DTO;
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

        public Task AddToCacheAsync(IEnumerable<ProductWithAttributesDTO> products)
        {
            throw new NotImplementedException();
        }

        public Task DeleteFromCacheAsync(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        public Task UpdateToCacheAsync(IEnumerable<ProductWithAttributesDTO> products)
        {
            throw new NotImplementedException();
        }
    }
}
