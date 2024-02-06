using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Suket;
using Suket.Data;
using Suket.Models;
using System;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Stripe;
using System.Text.Json;
using System.Collections.Generic;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

//builder.WebHost.UseUrls("http://0.0.0.0:5000"); // この行を追加


/*
var kvUri = "https://emailsender.vault.azure.net/";

var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
KeyVaultSecret sendGridKey = await client.GetSecretAsync("SendGridAPIKey");
*/

static async Task<string> GetSendGridApiKeyFromAWSSecretsManager()
{
    string secretName = "MintSPORTS_secret";
    string region = "ap-northeast-1";

    IAmazonSecretsManager client = new AmazonSecretsManagerClient(Amazon.RegionEndpoint.GetBySystemName(region));
    GetSecretValueRequest request = new GetSecretValueRequest
    {
        SecretId = secretName,
        VersionStage = "AWSCURRENT"
    };
    GetSecretValueResponse response = await client.GetSecretValueAsync(request);

    // JSONからディクショナリに変換
    var secrets = JsonSerializer.Deserialize<Dictionary<string, string>>(response.SecretString);

    // SendGridAPIKeyの値を返す
    return secrets["SendGridAPIKey"];
}

// Add services to the container.
string? connectionString = builder.Configuration.GetConnectionString("ApplicationDbContext");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<UserAccount>()
    .AddRoles<IdentityRole>() // Add this
    .AddEntityFrameworkStores<ApplicationDbContext>();
    //.AddDefaultTokenProviders();  // add this line

// Add configuration for IdentityOptions
builder.Services.Configure<IdentityOptions>(options =>
{
    
    options.Password.RequireNonAlphanumeric = false;
    
    // Other settings...

    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";
});
builder.Services.AddControllers(options => options.UseDateOnlyTimeOnlyStringConverters());
builder.Services.AddControllersWithViews();

// Add RazorPages
builder.Services.AddRazorPages();
// Add SendGrid EmailSender as a Singleton service
/*
builder.Services.AddSingleton<ISuketEmailSender, EmailSender>(i =>
    new EmailSender(sendGridKey.Value));
*/

var sendGridKey = await GetSendGridApiKeyFromAWSSecretsManager();
builder.Services.AddSingleton<ISuketEmailSender, EmailSender>(_ => new EmailSender(sendGridKey));

// Add the notification service to the DI container
builder.Services.AddSingleton<INotificationService, NotificationService>();

builder.Services.AddHostedService<UpdatePostStatusBackgroundService>();

builder.Services.AddHostedService<TimedHostedService>();

// StripeTransferServiceとDailyTaskServiceをサービスコレクションに追加
builder.Services.AddHostedService<StripeTransferService>();

builder.Services.AddSingleton<AWSSecretsManagerService>();

// HTTPSリダイレクションの設定を追加
/*
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode = (int)HttpStatusCode.PermanentRedirect;
        options.HttpsPort = 443;
    });
}
*/

// HSTSの設定を追加
/*
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
    // options.ExcludedHosts.Add("example.com");
    // options.ExcludedHosts.Add("www.example.com");
});
*/

//builder.Services.Configure<DataProtectionTokenProviderOptions>(opts => opts.TokenLifespan = TimeSpan.FromHours(10));


var app = builder.Build();

// 本番環境では、HSTSを使用
//app.UseHsts();

// HTTPSリダイレクトを使用
//app.UseHttpsRedirection();

// Add the seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<UserAccount>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await DataSeeder.SeedDataAsync(userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "userRegistration",
    pattern: "Users/Register",
    defaults: new { controller = "Users", action = "Register" });

app.MapControllerRoute(
    name: "isUserNameInUse",
    pattern: "Users/IsUserNameInUse",
    defaults: new { controller = "Users", action = "IsUserNameInUse" });

app.MapControllerRoute(
    name: "isEmailInUse",
    pattern: "Users/IsEmailInUse",
    defaults: new { controller = "Users", action = "IsEmailInUse" });

app.MapControllerRoute(
    name: "isEmailAvailable",
    pattern: "Users/IsEmailAvailable",
    defaults: new { controller = "Users", action = "IsEmailAvailable" });

app.MapControllerRoute(
    name: "userProfile",
    pattern: "Users/Balance",
    defaults: new { controller = "Users", action = "Balance" });

app.MapControllerRoute(
    name: "userProfile",
    pattern: "Users/{userName}",
    defaults: new { controller = "Users", action = "Profile" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
