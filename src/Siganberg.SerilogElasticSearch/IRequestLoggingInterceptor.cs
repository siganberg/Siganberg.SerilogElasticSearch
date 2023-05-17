using Microsoft.AspNetCore.Http;

namespace Siganberg.SerilogElasticSearch;

public interface IRequestLoggingInterceptor
{
    bool IncludeRequestWhen(HttpContext context);
}