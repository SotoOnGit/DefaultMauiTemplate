using Configuration;
using Configuration.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DefaultTemplateApp.Configs
{
    public class AppConfig : IDefaultTemplateConfiguration
    {
        private readonly ILogger<AppConfig> _logger;

        public AppConfig(ILogger<AppConfig> logger)
        {
            _logger = logger;

            using var fileStream = FileSystem.Current.OpenAppPackageFileAsync("config.json").Result;
            var content = new StreamReader(fileStream).ReadToEnd();
            StaticConfig = JsonConvert.DeserializeObject<StaticConfig>(content);
        }

        public AuthConfig Auth
        {
            get => GetObject<AuthConfig>("auth.data");
            set => PutObject("auth.data", value);
        }

        public string FirebaseToken
        {
            get => Preferences.Default.Get("firebaseToken", string.Empty);
            set => Preferences.Default.Set("firebaseToken", value);
        }

        public StaticConfig StaticConfig { get; }

        public void Log(string information)
        {
            _logger.LogInformation(information);
        }

        public bool IsInternetConnection()
        {
            return Connectivity.NetworkAccess == NetworkAccess.Internet;
        }

        private static T GetObject<T>(string key) where T : new()
        {
            var data = Preferences.Default.Get(key, string.Empty);
            return data == string.Empty ? new T() : JsonConvert.DeserializeObject<T>(data);
        }

        private static void PutObject<T>(string key, T value) where T : new()
        {
            var data = JsonConvert.SerializeObject(value);
            Preferences.Default.Set(key, data);
        }
    }
}
