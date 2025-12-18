using Delivery.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace WebhookReceiver.Controllers
{
    [ApiController]
    [Route("api/receiver")]
    public class ProductStructureWebhookController : ControllerBase
    {
        private readonly ProductStructureService _productStructureService;

        public ProductStructureWebhookController(ProductStructureService productStructureService)
        {
            _productStructureService = productStructureService;
        }

        [HttpPost("productStructureUpdate")]
        public async Task<IActionResult> ProductStructureUpdate([FromHeader(Name = "x-event-key")] string eventType, [FromBody] JsonElement payload)
        {
            await _productStructureService.HandleWebhookAsync(eventType, payload);

            return Accepted();
        }
    }
}
