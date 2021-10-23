
namespace SchoolMngr.Infrastructure.Shared.Configuration
{
    using Codeit.NetStdLibrary.Base.Common;
    using Microsoft.Extensions.Configuration;
    using Serilog;

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

    }
}
