using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using ToiletGhost.Components;
using ToiletGhost.Services;
using ToiletGhost.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add Azure AD authentication
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

// Add authorization
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddMicrosoftIdentityConsentHandler();

// Add SignalR
builder.Services.AddSignalR();

// Add custom services
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<ITableStorageService, TableStorageService>();
builder.Services.AddScoped<IFileValidationService, FileValidationService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IImageProcessingService, ImageProcessingService>();

// Add controllers for authentication
builder.Services.AddControllers()
    .AddMicrosoftIdentityUI();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .RequireAuthorization();

// Map SignalR hub
app.MapHub<ProjectHub>("/projecthub");

// Map controllers for authentication
app.MapControllers();

app.Run();
