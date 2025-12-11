using Delivery.Domain.DTO;
using Struct.App.Api.Models.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Application.Interfaces.Repositories
{
    public interface IProductWriteRepository
    {
        Task CacheUpdates(IEnumerable<ProductWithAttributesDTO> products);
    }
}
