using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Siganberg.SerilogElasticSearch;

public class DefaultRequestLoggingInterceptor : IRequestLoggingInterceptor
{
    public static DefaultRequestLoggingInterceptor Instance = new DefaultRequestLoggingInterceptor();
    
    private readonly List<string> _excludedPaths = new List<string>
    {
        "health",
        "swagger",
        "metrics"
    };

    public bool IncludeRequestWhen(HttpContext context)
    {
        var path = context.Request.Path.ToString().ToLower();
        return !_excludedPaths.Any(a => path.Contains(a));
    }
}