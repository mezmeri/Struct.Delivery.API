using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Delivery.Application.Services;
using Microsoft.AspNetCore.Authorization;
using System.Runtime.CompilerServices;

namespace WebhookReceiver.Controllers
{
    [ApiController]
    [Route("api/receiver")]
    public class VariantWebhookController : ControllerBase
    {
        private readonly VariantService _variantService;
        private readonly ILogger<VariantWebhookController> _logger;

        public VariantWebhookController(VariantService variantService, ILogger<VariantWebhookController> logger)
        {
            _variantService = variantService;
            _logger = logger;
        }

        [HttpPost("variantUpdate")]
        public async Task<IActionResult> VariantUpdate([FromHeader(Name = "x-event-key")] string eventType, [FromBody] JsonElement payload)
        {
            try
            {
                _logger.LogInformation("Received Variant webhook. Payload length: {Len}", payload.GetRawText()?.Length ?? 0);
                _logger.LogDebug("Variant payload: {Payload}", payload.GetRawText());

                await _variantService.HandleWebhookAsync(payload, eventType);

                _logger.LogInformation("Variant webhook handled and accepted.");
                return Accepted();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Variant webhook.");
                return StatusCode(500);
            }
        }
    }
}
