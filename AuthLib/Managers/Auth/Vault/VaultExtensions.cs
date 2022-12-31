using AuthLib.Managers.Auth.Vault.ProjectApi.CustomOptions;
using Microsoft.Extensions.Configuration;

namespace AuthLib.Managers.Auth.Vault
{
    public static class VaultExtensions
    {
        public static IConfigurationBuilder AddVault(this IConfigurationBuilder configuration,
        Action<VaultOptions> options)
        {
            var vaultOptions = new VaultConfigurationSource(options);
            configuration.Add((IConfigurationSource)vaultOptions);
            return configuration;
        }
    }
}
