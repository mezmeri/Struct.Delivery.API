using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Integration.Models
{
    public record ProductRequestModel
    {
        [JsonProperty("ProductIds")]
        public List<string> ProductsIds { get; set; }
    }
}
