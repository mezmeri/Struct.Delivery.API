using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebhookReceiver.Services;
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
        public async Task<IActionResult> ProductUpdate([FromBody] JsonElement payload)
        {
            JsonElement productChanges = payload.GetProperty("ProductChanges");

            foreach (JsonElement change in productChanges.EnumerateArray())
            {
                if (change.TryGetProperty("Id", out var idElement))
                {
                    string productId = idElement.GetInt32().ToString();

                    await _productService.EnqueueUpdateAsync(productId);
                }
            }
            return Accepted();
        }
    }
}
