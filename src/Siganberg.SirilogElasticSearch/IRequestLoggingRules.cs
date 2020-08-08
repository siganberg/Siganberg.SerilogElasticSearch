using Microsoft.AspNetCore.Http;

namespace Siganberg.SirilogElasticSearch
{
    public interface IRequestLoggingRules
    {
        bool Evaluate(HttpContext context);
    }
}