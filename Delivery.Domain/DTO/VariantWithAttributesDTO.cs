using Struct.App.Api.Models.Product;
using Struct.App.Api.Models.Variant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Domain.DTO
{
    public class VariantWithAttributesDTO
    {
        public VariantModel Variant { get; set; }
        public Dictionary<string, object> AttributeValues { get; set; }
    }
}
