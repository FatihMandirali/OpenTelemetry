using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Logging.Shared;

public class OpenTelemetryTraceIdMiddleware
{
    private readonly RequestDelegate _next;

    public OpenTelemetryTraceIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<OpenTelemetryTraceIdMiddleware>>();
        var traceId = Activity.Current.TraceId.ToString();
        //NOT: {@test} şeklimnde isimlendirme yaparsak eğer elastic tarafı bu değişkeni indexler,field olarak algılar ve böylece direkt test:1 şeklinde arama yapabilir hale geliriz. 
        using (logger.BeginScope("{@traceId}",traceId))
        {
            await _next(context);
        }
    }
}