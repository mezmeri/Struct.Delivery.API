using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Application.Models
{
    public class EntityItem
    {
        
        public string Id { get; set; }

        public long Timestamp { get; set; }

        // Tiny dictionary til ændringer
        // String for Alias,
        // Object for værdi (kan være string, int, bool etc)
        public Dictionary<string, object> ChangedAttributes { get; set; } = new Dictionary<string, object>();

       // Need this later maybe? 
        public string EnityModelType { get; set; }
    }
}
