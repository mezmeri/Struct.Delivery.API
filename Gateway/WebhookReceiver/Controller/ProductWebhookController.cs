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
        public async Task<IActionResult> ProductUpdate([FromBody] JsonElement payload)
        {
            var productIdsToUpdate = new List<string>();

            if (payload.TryGetProperty("ProductChanges", out var productChanges))
            {
                foreach (JsonElement change in productChanges.EnumerateArray())
                {
                    if (change.TryGetProperty("Id", out var idElement))
                    {
                        productIdsToUpdate.Add(idElement.ToString());
                    }
                }
            }

            if (productIdsToUpdate.Any())
            {
                await _productService.EnqueueUpdatesAsync(productIdsToUpdate);
            }

            return Accepted();
        }
    }
}
