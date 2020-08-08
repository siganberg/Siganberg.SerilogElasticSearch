using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Siganberg.SirilogElasticSearch.SampleApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthCheckController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HealthCheckController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public Task<JsonResult> Index()
        {
            _logger.LogInformation("Healthy");
            return Task.FromResult(new JsonResult(new { Status = "Healthy "}));
        }
    }
}