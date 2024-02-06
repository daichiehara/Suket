using Amazon;
using Amazon.SecretsManager.Model;
using Amazon.SecretsManager;
using System.Text.Json;

namespace Suket
{
    public class AWSSecretsManagerService
    {
        public async Task<string> GetSecretAsync(string secretName)
        {
            string region = "ap-northeast-1";
            IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));
            GetSecretValueRequest request = new GetSecretValueRequest
            {
                SecretId = secretName,
                VersionStage = "AWSCURRENT"
            };

            try
            {
                var response = await client.GetSecretValueAsync(request);
                var secrets = JsonSerializer.Deserialize<Dictionary<string, string>>(response.SecretString);
                //test
                //string name = "TestPay";
                string name = "StripeAPIKey";
                if (secrets != null && secrets.ContainsKey(name))
                {
                    return secrets[name];
                }

                throw new Exception("Secret key not found.");
            }
            catch (Exception e)
            {
                throw new Exception($"Error retrieving secret: {e.Message}");
            }
        }
    }
}
