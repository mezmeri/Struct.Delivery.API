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

        private readonly ILogger<ProductWebhookController> _logger;

        public ProductWebhookController(ProductService productService, ILogger<ProductWebhookController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpPost("productUpdate")]
        public async Task<IActionResult> ProductUpdate([FromBody] JsonElement payload)
        {
            _logger.LogInformation("Received product update webhook: {Payload}", payload.ToString());
            await _productService.HandleWebhookAsync(payload);

            return Accepted();
        }
    }
}
