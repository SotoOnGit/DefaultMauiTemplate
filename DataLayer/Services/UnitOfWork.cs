using Configuration;

namespace DataLayer.Services
{
    public class UnitOfWork
    {
        private readonly IDefaultTemplateConfiguration _configuration;

        public UnitOfWork(IDefaultTemplateConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool HasCredentials()
        {
            return !string.IsNullOrEmpty(_configuration.Auth.RefreshToken) && !string.IsNullOrEmpty(_configuration.Auth.AccessToken);
        }

        public void Logout()
        {

        }
    }
}
