using Struct.App.Api.Models.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Domain.DTO
{
    public class ProductWithAttributesDTO
    {
        public ProductModel Product { get; set; }
        public Dictionary<string, object> AttributeValues { get; set; }
    }
}
