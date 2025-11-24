using Delivery.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebhookReceiver.Controllers
{
    [ApiController]
    public class ProductController : Controller
    {
        private readonly QueueService _queueService;
        private readonly ILogger<ProductController> _logger;
        public ProductController(QueueService queueService, ILogger<ProductController> logger)
        {
            _queueService = queueService;
            _logger = logger;
        }

        [HttpGet]
        [Route("products")]
        public IActionResult Index()
        {
            return Ok();
        }
    }
}
