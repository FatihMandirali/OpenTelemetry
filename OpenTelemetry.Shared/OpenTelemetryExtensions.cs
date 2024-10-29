using System.Diagnostics;
using MassTransit.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace OpenTelemetry.Shared;

public static class OpenTelemetryExtensions
{
    public static void AddOpenTelemetryExt(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.Configure<OpenTelemetryConstants>(configuration.GetSection("OpenTelemetry"));

        var openTelemetryConstants = configuration.GetSection("OpenTelemetry").Get<OpenTelemetryConstants>();
        
        ActivitySourceProvider.Source = new ActivitySource(openTelemetryConstants.ActivitySourceName);

        serviceCollection.AddOpenTelemetry().WithTracing(options =>
        {
            options.AddSource(openTelemetryConstants.ActivitySourceName)
                .AddSource(DiagnosticHeaders.DefaultListenerName)
                .ConfigureResource(resource =>
                {
                    resource.AddService(openTelemetryConstants.ServiceName,serviceVersion:openTelemetryConstants.ServiceVersion);
                });
            
            options.AddAspNetCoreInstrumentation(aspnetcoreoptions =>
            {
                aspnetcoreoptions.Filter = (context) => context.Request.Path.Value.Contains("api",StringComparison.InvariantCulture);
                aspnetcoreoptions.RecordException = true;
                aspnetcoreoptions.EnrichWithException = (activity, exception) =>
                {
                    activity.SetTag("özel hata tag", exception);
                    //HER EXCEPTİONDAN SONRA DÜŞER.
                };
            });
            options.AddEntityFrameworkCoreInstrumentation(efcoreOptions =>
            {
                efcoreOptions.SetDbStatementForText = true;
                efcoreOptions.SetDbStatementForStoredProcedure = true;
                efcoreOptions.EnrichWithIDbCommand = (activity,dbCommand) =>
                {
                    //ÖZEL OLARAK COMMANDLARI YAKALAMAK İÇİN KULLANILIR.
                    //HER EF İŞLEMİNDEN SONRA BURAYA DÜŞER
                };
            });
            options.AddHttpClientInstrumentation(httpOptions =>
            {
                httpOptions.EnrichWithHttpRequestMessage = async (activity, request) =>
                {
                    var requestContent = "empty";

                    if (request.Content != null)
                    {
                        requestContent = await request.Content.ReadAsStringAsync();
                    }

                    activity.SetTag("http.request.body", requestContent);
                };
                httpOptions.EnrichWithHttpResponseMessage = async (activity, response) =>
                {
                    if (response.Content != null)
                    {
                        activity.SetTag("http.response.body", await response.Content.ReadAsStringAsync());
                    }
                };
            });
            options.AddRedisInstrumentation(redisOptions =>
            {
                redisOptions.SetVerboseDatabaseStatements = true;
            });
            options.AddConsoleExporter();
            options.AddOtlpExporter(); //Jeager

        });
    }
}
