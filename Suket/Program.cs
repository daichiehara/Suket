using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Suket;
using Suket.Data;
using Suket.Models;
using System;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

var builder = WebApplication.CreateBuilder(args);

var kvUri = "https://emailsender.vault.azure.net/";

var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
KeyVaultSecret sendGridKey = await client.GetSecretAsync("SendGridAPIKey");

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
    // Other settings...

    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";
});
builder.Services.AddControllers(options => options.UseDateOnlyTimeOnlyStringConverters());
builder.Services.AddControllersWithViews();

// Add RazorPages
builder.Services.AddRazorPages();
// Add SendGrid EmailSender as a Singleton service
builder.Services.AddSingleton<ISuketEmailSender, EmailSender>(i =>
    new EmailSender(sendGridKey.Value));

// Add the notification service to the DI container
builder.Services.AddSingleton<INotificationService, NotificationService>();

builder.Services.AddHostedService<UpdatePostStatusBackgroundService>();


var app = builder.Build();

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
    name: "userProfile",
    pattern: "Users/{userName}",
    defaults: new { controller = "Users", action = "Profile" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
