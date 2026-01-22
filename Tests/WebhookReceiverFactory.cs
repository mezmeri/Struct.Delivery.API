using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Testing;
using Struct.Delivery.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delivery.Integration
{
    public class WebhookReceiverFactory : WebApplicationFactory<Program>
    {
        public WebhookReceiverFactory()
        {
        }
    }
}
