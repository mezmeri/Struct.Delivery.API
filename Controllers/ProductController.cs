using Microsoft.AspNetCore.Mvc;

namespace Struct.Delivery.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : Controller
    {
        [HttpGet]
        [Route("products")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
