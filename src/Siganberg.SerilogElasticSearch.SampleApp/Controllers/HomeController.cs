using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Siganberg.SerilogElasticSearch.SampleApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IHttpClientFactory _clientFactory;
        public HomeController(ILogger<HomeController> logger, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _clientFactory = clientFactory;
        }

        [HttpGet]
        public async Task<string> Index()
        {
            _logger.LogInformation("Test Logging....");

            var client = _clientFactory.CreateClient();
            var result = await client.GetAsync("https://jsonplaceholder.typicode.com/todos/1");
            var content = await result.Content.ReadAsStringAsync();
            return content;
        }
    }
}