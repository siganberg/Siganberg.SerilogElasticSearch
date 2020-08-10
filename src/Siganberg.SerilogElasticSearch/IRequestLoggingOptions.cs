using Microsoft.AspNetCore.Http;

namespace Siganberg.SerilogElasticSearch
{
    public interface IRequestLoggingOptions
    {
        bool IncludeRequestWhen(HttpContext context);
    }
}