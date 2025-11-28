using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Delivery.Application.Services;
using Microsoft.AspNetCore.Authorization;

namespace WebhookReceiver.Controllers
{
    [ApiController]
    [Route("api/receiver")]
    public class VariantWebhookController : ControllerBase
    {
        private readonly VariantService _variantService;

        public VariantWebhookController(VariantService variantService)
        {
            _variantService = variantService;
        }

    [HttpPost("variantUpdate")]
        public async Task<IActionResult> VariantUpdate([FromBody] JsonElement payload)
        {
            await _variantService.HandleWebhookAsync(payload);

            return Accepted();
        }
    }
}
