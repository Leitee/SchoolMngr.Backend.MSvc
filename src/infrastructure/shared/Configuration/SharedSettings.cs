/// <summary>
/// 
/// </summary>
namespace SchoolMngr.Infrastructure.Shared.Configuration
{
    using Pandora.NetStdLibrary.Base.Common;

    public class SharedSettings : BaseSettings<SharedSettings>
    {
        public EventBus EventBus { get; set; }
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
}
