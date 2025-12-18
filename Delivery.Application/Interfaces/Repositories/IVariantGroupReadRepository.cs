using Delivery.Domain.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Application.Interfaces.Repositories
{
    public interface IVariantGroupReadRepository
    {
        Task<List<VariantGroupWithAttributesDTO>> GetPimData(List<int> variantGroupIds);
    }
}
