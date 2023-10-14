using Microsoft.AspNetCore.Identity;
using Suket.Models;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System.Text.Json;
using System.Collections.Generic;

namespace Suket.Data
{
    public class DataSeeder
    {
        /*
        private static string GetAdminPasswordFromAzureKeyVault()
        {
            var keyVaultUrl = "https://adminsecretpassword.vault.azure.net/";
            var secretName = "AdminPassword";

            var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
            KeyVaultSecret adminPasswordSecret = client.GetSecret(secretName);

            return adminPasswordSecret.Value;
        }
        */

        private static async Task<string> GetAdminPasswordFromAWSSecretsManager()
        {
            string secretName = "MintSPORTS_secret";  // シークレット名を正しいものに変更
            string region = "ap-northeast-1";

            IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));

            GetSecretValueRequest request = new GetSecretValueRequest
            {
                SecretId = secretName,
                VersionStage = "AWSCURRENT", // VersionStage defaults to AWSCURRENT if unspecified.
            };

            GetSecretValueResponse response;

            try
            {
                response = await client.GetSecretValueAsync(request);
            }
            catch (Exception e)
            {
                // For a list of the exceptions thrown, see
                // https://docs.aws.amazon.com/secretsmanager/latest/apireference/API_GetSecretValue.html
                throw e;
            }

            // JSONからAdminPasswordを取得
            var secrets = JsonSerializer.Deserialize<Dictionary<string, string>>(response.SecretString);
            if (secrets != null && secrets.ContainsKey("AdminPassword"))
            {
                return secrets["AdminPassword"];
            }

            throw new Exception("AdminPassword not found in the secret.");
        }


        public static async Task SeedDataAsync(UserManager<UserAccount> userManager, RoleManager<IdentityRole> roleManager)
        {
            var adminRole = new IdentityRole("Admin");
            if (!await roleManager.RoleExistsAsync(adminRole.Name))
            {
                await roleManager.CreateAsync(adminRole);
            }

            var userRole = new IdentityRole("User");
            if (!await roleManager.RoleExistsAsync(userRole.Name))
            {
                await roleManager.CreateAsync(userRole);
            }

            var adminUser = new UserAccount { UserName = "Admin", Email = "ehara@roadmint.co.jp" };
            if (await userManager.FindByNameAsync(adminUser.UserName) == null)
            {
                var adminPassword = await GetAdminPasswordFromAWSSecretsManager();
                await userManager.CreateAsync(adminUser, adminPassword);
                await userManager.AddToRoleAsync(adminUser, adminRole.Name);
            }
        }
    }
}
