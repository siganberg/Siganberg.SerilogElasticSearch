using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Siganberg.SirilogElasticSearch.Middleware
{
    [ExcludeFromCodeCoverage]
    internal class CorrelationIdMiddleWare
    {
        const string RequestIdHeaderName = "x-request-id";

        private readonly RequestDelegate _next;
        public CorrelationIdMiddleWare(RequestDelegate next)
        {
            _next = next;
        }
        public Task InvokeAsync(HttpContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (string.IsNullOrWhiteSpace(context.Request.Headers[RequestIdHeaderName]))
                context.Request.Headers[RequestIdHeaderName] = Guid.NewGuid().ToString();

            context.TraceIdentifier = context.Request.Headers[RequestIdHeaderName];

            return _next(context);
        }
    }


}