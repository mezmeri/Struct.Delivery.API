using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Delivery.Application.Services;
using Microsoft.AspNetCore.Authorization;

namespace WebhookReceiver.Controllers
{
    [ApiController]
    [Route("api/receiver")]
    public class ProductWebhookController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductWebhookController(ProductService productService)
        {
            _productService = productService;
        }

        [HttpPost("productUpdate")]
        public async Task<IActionResult> ProductUpdate([FromHeader(Name = "x-event-key")] string eventType, [FromBody] JsonElement payload)
        {
            await _productService.HandleWebhookAsync(eventType, payload);

            return Accepted();
        }
    }
}
