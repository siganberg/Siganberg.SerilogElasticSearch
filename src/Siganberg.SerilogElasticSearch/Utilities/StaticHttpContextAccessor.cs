using Microsoft.AspNetCore.Http;

namespace Siganberg.SerilogElasticSearch.Utilities;

public static class StaticHttpContextAccessor
{
    private static IHttpContextAccessor _httpContextAccessor;

    public static void Configure(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    public static HttpContext Current => _httpContextAccessor?.HttpContext;
}