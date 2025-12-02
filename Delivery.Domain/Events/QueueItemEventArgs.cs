using Delivery.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Domain.Events
{
    public class QueueItemEventArgs : EventArgs
    {
        public string Id { get; set; }
        public string EventType { get; set; }
        public EntityType EntityType { get; set; }
        public long Timestamp { get; set; }
    }
}
