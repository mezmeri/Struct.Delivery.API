using Delivery.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Application.Interfaces
{
    public interface IEntityEventService
    {
        EntityType EntityType { get; }
        Task ProcessAsync(string eventType, List<string> ids);
    }
}
