using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Siganberg.SirilogElasticSearch.SampleApp
{
    public class RequestLoggingRules : IRequestLoggingRules
    {
        private readonly List<string> _excludedPaths = new List<string>
        {
            "HealthCheck"
        };
        public bool Evaluate(HttpContext context)
        {
            var path = context.Request.Path.ToString();
            if (_excludedPaths.Any(a => path.Contains(a)))
                return false;
            return true;
        }
    }
}