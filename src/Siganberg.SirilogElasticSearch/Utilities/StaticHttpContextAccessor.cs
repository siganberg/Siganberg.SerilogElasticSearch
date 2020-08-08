using Microsoft.AspNetCore.Http;

namespace Siganberg.SirilogElasticSearch.Utilities
{
    /// <summary>
    ///
    /// </summary>
    public static class StaticHttpContextAccessor

    {
        private static IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        ///
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        ///
        /// </summary>
        public static HttpContext Current => _httpContextAccessor?.HttpContext;
    }
}