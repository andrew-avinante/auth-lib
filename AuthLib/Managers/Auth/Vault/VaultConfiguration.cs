namespace AuthLib.Managers.Auth.Vault
{
    using System;
    using System.Threading.Tasks;
    using VaultSharp;
    using VaultSharp.V1.AuthMethods.AppRole;
    using VaultSharp.V1.Commons;
    using Microsoft.Extensions.Configuration;

    namespace ProjectApi.CustomOptions
    {
        public class VaultConfigurationProvider : ConfigurationProvider
        {
            public VaultOptions _config;
            private IVaultClient _client;

            public VaultConfigurationProvider(VaultOptions config)
            {
                _config = config;

                var vaultClientSettings = new VaultClientSettings(
                    _config.Address,
                    new AppRoleAuthMethodInfo(_config.Role,
                                              _config.Secret)
                );

                _client = new VaultClient(vaultClientSettings);
            }

            public override void Load()
            {
                LoadAsync().Wait();
            }

            public async Task LoadAsync()
            {
                await GetDatabaseCredentials();
            }

            public async Task GetDatabaseCredentials()
            {
                var userID = "";
                var password = "";

                if (_config.SecretType == "secrets")
                {
                    Secret<SecretData> secrets = await _client.V1.Secrets.KeyValue.V2.ReadSecretAsync(
                      "services/crooked-cactus-api", null, _config.MountPath);

                    userID = secrets.Data.Data["db_username"].ToString();
                    password = secrets.Data.Data["db_password"].ToString();
                }

                Data.Add("database:userID", userID);
                Data.Add("database:password", password);
            }
        }
    }
}
