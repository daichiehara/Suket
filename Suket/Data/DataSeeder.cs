using Microsoft.AspNetCore.Identity;
using Suket.Models;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;

namespace Suket.Data
{
    public class DataSeeder
    {
        private static string GetAdminPasswordFromAzureKeyVault()
        {
            var keyVaultUrl = "https://adminsecretpassword.vault.azure.net/";
            var secretName = "AdminPassword";

            var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
            KeyVaultSecret adminPasswordSecret = client.GetSecret(secretName);

            return adminPasswordSecret.Value;
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

            var adminUser = new UserAccount { UserName = "Admin", Email = "admin@example.com" };
            if (await userManager.FindByNameAsync(adminUser.UserName) == null)
            {
                var adminPassword = GetAdminPasswordFromAzureKeyVault();
                await userManager.CreateAsync(adminUser, adminPassword);
                await userManager.AddToRoleAsync(adminUser, adminRole.Name);
            }
        }
    }
}
