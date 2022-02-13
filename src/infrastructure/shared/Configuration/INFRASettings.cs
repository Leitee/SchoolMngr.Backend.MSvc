
namespace SchoolMngr.Infrastructure.Shared.Configuration;

public class INFRASettings : BaseSettings
{
    public const string SectionKey = "InfraSection";

    public EventBusSection? EventBus { get; set; }
    public DigestLoggerSection? DigestLogger { get; set; }
    public IdentityServerSection? IdentityServer { get; set; }
    public bool EnableDetailedDebug { get; set; }

    public override string ToString()
    {
        return SectionKey;
    }
}

public class IdentityServerSection
{
    public string? JwtSecretKey { get; set; }
    public string? JwtValidIssuer { get; set; }
    public string? JwtValidAudience { get; set; }
}

public class DigestLoggerSection
{
    public bool UseLogger { get; set; }
    public string? ServerUrl { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }

}

public class EventBusSection
{
    public bool UseEventBus { get; set; }
    public string? Host { get; set; }
    public int Port { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? QuequeName { get; set; }
    public int RetryCount { get; set; }
}
