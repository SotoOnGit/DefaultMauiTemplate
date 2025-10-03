using Configuration.Models;

namespace Configuration
{
    public interface IDefaultTemplateConfiguration
    {
        public AuthConfig Auth { get; set; }
        public string FirebaseToken { get; set; }

        public StaticConfig StaticConfig { get; }
        public Stream DefaultImage { get; }

        public void Log(string information);
        public bool IsInternetConnection();
    }
}
