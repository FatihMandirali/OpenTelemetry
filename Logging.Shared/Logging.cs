using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Exceptions;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.Elasticsearch;

namespace Logging.Shared;

public static class Logging
{
    public static Action<HostBuilderContext, LoggerConfiguration> ConfigurationLogging =>
        (builderContext, loggerConfiguration) =>
        {
            var environment = builderContext.HostingEnvironment;

            loggerConfiguration
                .ReadFrom.Configuration(builderContext.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithProperty("Env",environment.EnvironmentName)
                .Enrich.WithProperty("AppName",environment.ApplicationName)
                ;

            var elasticSeachBaseUrl = builderContext.Configuration.GetSection("Elasticsearch")["BaseUrl"];
            var elasticSeachUsername = builderContext.Configuration.GetSection("Elasticsearch")["Username"];
            var elasticSeachPassword = builderContext.Configuration.GetSection("Elasticsearch")["Password"];
            var elasticSeachIndexName = builderContext.Configuration.GetSection("Elasticsearch")["IndexName"];

            loggerConfiguration.WriteTo.Elasticsearch(new(new Uri(elasticSeachBaseUrl))
            {
                AutoRegisterTemplate = true,
                AutoRegisterTemplateVersion = Serilog.Sinks.Elasticsearch.AutoRegisterTemplateVersion.ESv8,
                IndexFormat = $"{elasticSeachIndexName}-{environment.EnvironmentName}-logs-"+"{0:yyy.MM.dd}",
                ModifyConnectionSettings = x=>x.BasicAuthentication(elasticSeachUsername,elasticSeachPassword),
                CustomFormatter = new ElasticsearchJsonFormatter()
            });

        };
}