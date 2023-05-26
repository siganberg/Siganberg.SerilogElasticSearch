using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Siganberg.SerilogElasticSearch.SampleApp;

public class RequestLoggingInterceptor : IRequestLoggingInterceptor
{
    private readonly List<string> _excludedPaths = new List<string>
    {
        "healthz",
        "swagger"
    };

    public bool IncludeRequestWhen(HttpContext context)
    {
        var path = context.Request.Path.ToString().ToLower();
        if (_excludedPaths.Any(a => path.Contains(a)))
            return false;
        
        // TODO: You can add more logic here to determine if request logging will be included. 
        return true; 
    }

}