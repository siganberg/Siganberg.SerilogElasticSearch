using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Logging;

namespace Siganberg.SerilogElasticSearch.SampleApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : Controller
    {

        [HttpGet]
        public async Task<JsonResult> Index()
        {

            var model = new
            {
                Description = "color=\"#23456\""
            };

            return await Task.FromResult(new JsonResult(model));
        }
    }


}