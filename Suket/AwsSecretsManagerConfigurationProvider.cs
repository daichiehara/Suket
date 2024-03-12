using Amazon.SecretsManager.Model;
using Amazon.SecretsManager;
using System.Text.Json;

namespace Suket
{
    public class AwsSecretsManagerConfigurationProvider : ConfigurationProvider
    {
        private readonly string _secretId;
        private readonly AmazonSecretsManagerClient _client;

        public AwsSecretsManagerConfigurationProvider(string secretId, AmazonSecretsManagerClient client)
        {
            _secretId = secretId;
            _client = client;
        }

        public override void Load()
        {
            var request = new GetSecretValueRequest
            {
                SecretId = _secretId
            };

            var response = _client.GetSecretValueAsync(request).Result;
            if (!string.IsNullOrEmpty(response.SecretString))
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, string>>(response.SecretString);
                foreach (var pair in data)
                {
                    Data.Add(pair.Key, pair.Value);
                }
            }
        }
    }
}
