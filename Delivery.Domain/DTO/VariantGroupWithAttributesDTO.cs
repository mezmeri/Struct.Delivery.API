using Struct.App.Api.Models.Product;
using Struct.App.Api.Models.VariantGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Domain.DTO
{
    public class VariantGroupWithAttributesDTO
    {
        public VariantGroupModel VariantGroup { get; set; }
        public Dictionary<string, object> AttributeValues { get; set; }
    }
}
