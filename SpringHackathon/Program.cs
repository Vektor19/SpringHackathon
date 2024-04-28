using SpringHackathon.Models;
using SpringHackathon.Settings;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
var builder = WebApplication.CreateBuilder(args);
var mongoDbSettings = builder.Configuration.GetSection(nameof(MongoDbConfig)).Get<MongoDbConfig>();
// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddIdentity<User, UserRole>()
        .AddMongoDbStores<User, UserRole, Guid>
        (
            mongoDbSettings.ConnectionString, mongoDbSettings.Name
        );

builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "User API", Version = "v1" });
});
//Google auth
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration.GetSection("GoogleAuthSettings").GetValue<string>("ClientId");
        options.ClientSecret = builder.Configuration.GetSection("GoogleAuthSettings").GetValue<string>("ClientSecret");
    })
     .AddTwitter(options =>
     {
         options.ConsumerKey = builder.Configuration.GetSection("TwitterAuthSettings").GetValue<string>("ApiKey");
         options.ConsumerSecret = builder.Configuration.GetSection("TwitterAuthSettings").GetValue<string>("ApiKeySecret");
         options.RetrieveUserDetails = true;
     })
     .AddDiscord(options =>
     {
         options.ClientId = builder.Configuration.GetSection("DiscordAuthSettings").GetValue<string>("ClientId");
         options.ClientSecret = builder.Configuration.GetSection("DiscordAuthSettings").GetValue<string>("ClientSecret");
         options.Scope.Add("identify");
         options.Scope.Add("email");
         options.SaveTokens = true;
     })
     .AddGitHub(options =>
     {
         options.ClientId = builder.Configuration.GetSection("GitHubAuthSettings").GetValue<string>("ClientId");
         options.ClientSecret = builder.Configuration.GetSection("GitHubAuthSettings").GetValue<string>("ClientSecret");
         options.Scope.Add("user:email");
         options.Scope.Add("read:user");
         options.SaveTokens = true;
     });
     

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseSwagger();



app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.UseSwaggerUI(c =>
{
	c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api");
});
app.Run();
