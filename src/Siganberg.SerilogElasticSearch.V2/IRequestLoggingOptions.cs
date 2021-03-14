using Microsoft.AspNetCore.Http;

namespace Siganberg.SerilogElasticSearch.V2
{
    public interface IRequestLoggingOptions
    {
        bool IncludeRequestWhen(HttpContext context);
    }
}