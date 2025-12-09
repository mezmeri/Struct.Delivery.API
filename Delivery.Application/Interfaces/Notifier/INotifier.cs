using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Application.Interfaces.Notifier
{
    public interface INotifier
    {
        Task NotifyChangesAsync(IEnumerable<string> ids, string eventType, DateTimeOffset timestamp, string entityType);
    }
}
