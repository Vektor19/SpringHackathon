using SpringHackathon.Models;
using SpringHackathon.Settings;
using Microsoft.OpenApi.Models;
using SpringHackathon.Services;
using SpringHackathon.Hubs;
using System.Reflection;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);
var mongoDbSettings = builder.Configuration.GetSection(nameof(MongoDbConfig)).Get<MongoDbConfig>();

builder.Services.Configure<EmailSenderConfig>(builder.Configuration.GetSection("EmailSenderConfig"));
builder.Services.AddSingleton<EmailSenderService>(service => new EmailSenderService(builder.Configuration.GetSection(nameof(EmailSenderConfig)).Get<EmailSenderConfig>()));

builder.Services.AddControllersWithViews();

builder.Services.AddSignalR();
builder.Services.AddIdentity<User, UserRole>()
		.AddMongoDbStores<User, UserRole, Guid>
        (
            mongoDbSettings.ConnectionString, mongoDbSettings.Name
        );
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
});
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "User API", Version = "v1" });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    
    app.UseHsts();
}
app.UseSwagger();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.MapHub<ChatHub>("/chatHub");
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.UseSwaggerUI(c =>
{
	c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api");
});
app.Run();
