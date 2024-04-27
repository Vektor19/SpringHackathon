using SpringHackathon.Models;
using SpringHackathon.Settings;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
var builder = WebApplication.CreateBuilder(args);
var mongoDbSettings = builder.Configuration.GetSection(nameof(MongoDbConfig)).Get<MongoDbConfig>();
// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddIdentity<User, UserRole>()
        .AddMongoDbStores<User, UserRole, Guid>
        (
            mongoDbSettings.ConnectionString, mongoDbSettings.Name
        );

    
//Google auth
/*builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = "your-google-client-id";
        options.ClientSecret = "your";
    })
    .AddTwitter(options =>
    {
        options.ConsumerSecret = "your-twitter-consumer-secret";
    })
    .AddFacebook(options =>
    {
        options.ClientId = "your-facebook-client-id";
        options.ClientSecret = "your-facebook-client-secret";
    });*/
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
