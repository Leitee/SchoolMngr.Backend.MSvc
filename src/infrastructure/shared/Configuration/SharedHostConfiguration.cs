
namespace SchoolMngr.Infrastructure.Shared.Configuration;

public static class SharedHostConfiguration
{
    public const string ApplicationName = nameof(ApplicationName);

    public static Serilog.ILogger CreateSerilogLogger(string appName)
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
