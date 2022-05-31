using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Siganberg.SerilogElasticSearch.SampleApp
{
    public class RequestLoggingOptions : IRequestLoggingOptions
    {
        private readonly List<string> _excludedPaths = new List<string>
        {
            "healthz",
            "swagger"
        };

        public bool IncludeRequestWhen(HttpContext context)
        {
            var path = context.Request.Path.ToString();
            if (_excludedPaths.Any(a => path.Contains(a)))
                return false;
            return true;
        }

    }
}