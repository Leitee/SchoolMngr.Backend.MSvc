
namespace SchoolMngr.Infrastructure.Shared.Configuration;

public class INFRASettings : BaseSettings
{
    public const string SectionKey = "InfraSection";

    public EventBus EventBus { get; set; }
    public DigestLogger DigestLogger { get; set; }
    public IdentityServer IdentityServer { get; set; }
    public bool EnableDetailedDebug { get; set; }

    public override string ToString()
    {
        return SectionKey;
    }
}

public class IdentityServer
{
    public string JwtSecretKey { get; set; }
    public string JwtValidIssuer { get; set; }
    public string JwtValidAudience { get; set; }
}

public class DigestLogger
{
    public bool UseLogger { get; set; }
    public string ServerUrl { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }

}

public class EventBus
{
    public bool UseEventBus { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string QuequeName { get; set; }
    public int RetryCount { get; set; }
}
