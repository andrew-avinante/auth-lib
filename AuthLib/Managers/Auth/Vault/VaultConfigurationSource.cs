using AuthLib.Managers.Auth.Vault.ProjectApi.CustomOptions;
using Microsoft.Extensions.Configuration;

namespace AuthLib.Managers.Auth.Vault
{
    public class VaultConfigurationSource : IConfigurationSource
    {
        private VaultOptions _config;

        public VaultConfigurationSource(Action<VaultOptions> config)
        {
            _config = new VaultOptions();
            config.Invoke(_config);
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new VaultConfigurationProvider(_config);
        }
    }
}
