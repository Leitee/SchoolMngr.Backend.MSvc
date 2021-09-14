
namespace SchoolMngr.Infrastructure.Shared.Configuration
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Serilog;
    using System.IO;

    public static class SharedHostConfiguration
    {
        private static string environmentName = EnvironmentName.Development;

        public static ILogger CreateSerilogLogger(string appName)
        {
            //var seqServerUrl = configuration["Serilog:SeqServerUrl"];
            //var logstashUrl = configuration["Serilog:LogstashgUrl"];
            return new LoggerConfiguration()
                .Enrich.WithProperty("ApplicationName", appName)
                .Enrich.FromLogContext()
                //.WriteTo.Seq(string.IsNullOrWhiteSpace(seqServerUrl) ? "http://seq" : seqServerUrl)
                //.WriteTo.Http(string.IsNullOrWhiteSpace(logstashUrl) ? "http://logstash:8080" : logstashUrl)
                .ReadFrom.Configuration(GetBasicConfiguration())
                .CreateLogger();
        }

        public static IConfiguration GetBasicConfiguration()
        {
            return BuildDefaultSettings(new ConfigurationBuilder());
        }

        public static IConfiguration BuildDefaultSettings(IConfigurationBuilder configurationBuilder)
        {
            var basePath = Directory.GetCurrentDirectory();
            var configuration = configurationBuilder
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            return configuration;
        }
    }
}
