using Struct.App.Api.Models.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Application.Interfaces.Repositories
{
    public interface IProductReadRepository
    {
        Task<IEnumerable<(string, long)>> GetProductChanges();

        Task RemoveFromQueueAsync(IEnumerable<string> ids);

        Task<IEnumerable<ProductModel>> GetPimData(IEnumerable<string> ids);

        Task CacheUpdates(IEnumerable<ProductModel> products);
    }
}
