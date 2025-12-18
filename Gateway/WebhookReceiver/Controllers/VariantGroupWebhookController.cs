using Delivery.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace WebhookReceiver.Controllers
{
    [ApiController]
    [Route("api/receiver")]
    public class VariantGroupWebhookController : Controller
    {
        private readonly VariantGroupService _variantGroupService;

        public VariantGroupWebhookController(VariantGroupService variantGroupService)
        {
            _variantGroupService = variantGroupService;
        }

        [HttpPost("variantGroupUpdate")]
        public async Task<IActionResult> VariantGroupUpdate([FromHeader(Name = "x-event-key")] string eventType, [FromBody] JsonElement payload)
        {
            await _variantGroupService.HandleWebhookAsync(eventType, payload);

            return Accepted();
        }
    }
}
