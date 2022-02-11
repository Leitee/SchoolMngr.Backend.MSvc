
namespace SchoolMngr.Infrastructure.Shared;

public static class ProgramConfiguration
{
    private static readonly string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

    public static Serilog.ILogger CreateSerilogLogger(IConfiguration configuration, string appName)
    {
        //var seqServerUrl = configuration["Serilog:SeqServerUrl"];
        //var logstashUrl = configuration["Serilog:LogstashgUrl"];
        return new LoggerConfiguration()
            .Enrich.WithProperty("ApplicationName", appName)
            .Enrich.WithProperty("Environment", environmentName)
            .Enrich.FromLogContext()
            //.WriteTo.Seq(string.IsNullOrWhiteSpace(seqServerUrl) ? "http://seq" : seqServerUrl)
            //.WriteTo.Http(string.IsNullOrWhiteSpace(logstashUrl) ? "http://logstash:8080" : logstashUrl)
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
    }

    public static IConfiguration GetConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }
}
