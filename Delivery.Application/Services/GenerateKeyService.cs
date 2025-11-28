using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Application.Services
{
    public class GenerateKeyService
    {
        public (string, string, string) GenerateQueueKey(string eventType)
        {
            string queueKey = $"{eventType}";

            return ($"{queueKey}:pending", $"{queueKey}:timestamps", $"{queueKey}:list");
        }

        public string ExtractEventType(string listKey)
        {
            string[] parts = listKey.Split(':');
            return $"{parts[0]}:{parts[1]}";
        }
    }
}
