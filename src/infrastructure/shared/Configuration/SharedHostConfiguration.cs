
namespace SchoolMngr.Infrastructure.Shared.Configuration
{
    using Codeit.NetStdLibrary.Base.Common;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Serilog;
    using Serilog.Events;
    using Serilog.Formatting.Elasticsearch;
    using Serilog.Sinks.Elasticsearch;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public static class SharedHostConfiguration
    {
        public const string ApplicationName = nameof(ApplicationName);

        public static ILogger CreateSerilogLogger(string appName)
        {
            return new LoggerConfiguration()
             .ReadFrom.Configuration(BuildDefaultSettings())
             .Enrich.WithProperty(ApplicationName, appName)
             .Enrich.WithEnvironmentName()
             .Enrich.WithMachineName()
             .Enrich.FromLogContext()
             .CreateLogger();
        }

        public static IConfiguration BuildDefaultSettings()
        {
            return BuildDefaultSettings(new ConfigurationBuilder());
        }

        public static IConfiguration BuildDefaultSettings(IConfigurationBuilder configurationBuilder)
        {
            return CodeitUtils.BuildDefaultSettings(configurationBuilder);
        }

        private static ElasticsearchSinkOptions ConfigureElasticSink(IConfiguration configuration, string environment = "Dev")
        {
            return new ElasticsearchSinkOptions(new Uri(configuration["Serilog:ElasticConfiguration"]))
            {
                BufferCleanPayload = (failingEvent, statuscode, exception) =>
                {
                    dynamic e = JObject.Parse(failingEvent);
                    return JsonConvert.SerializeObject(new Dictionary<string, object>()
                    {
                        { "@timestamp",e["@timestamp"]},
                        { "level","Error"},
                        { "message","Error: "+e.message},
                        { "messageTemplate",e.messageTemplate},
                        { "failingStatusCode", statuscode},
                        { "failingException", exception}
                    });
                },
                MinimumLogEventLevel = LogEventLevel.Verbose,
                AutoRegisterTemplate = true,
                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                CustomFormatter = new ExceptionAsObjectJsonFormatter(renderMessage: true),
                IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace(".", "-")}-{environment?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}",
                EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                                       EmitEventFailureHandling.WriteToFailureSink |
                                       EmitEventFailureHandling.RaiseCallback
            };
        }
    }
}
