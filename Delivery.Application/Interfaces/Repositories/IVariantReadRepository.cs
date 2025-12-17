using Delivery.Domain.DTO;
using Struct.App.Api.Models.Variant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Application.Interfaces.Repositories
{
    public interface IVariantReadRepository
    {
        Task<List<VariantWithAttributesDTO>> GetPimData(List<int> ids);
    }
}
