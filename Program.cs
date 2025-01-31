using Azure.Identity;
using Greenhouse.Components;
using Greenhouse.Config;
using Greenhouse.Data;
using Greenhouse.Data.Interfaces;
using Greenhouse.Data.Models;
using Greenhouse.Data.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Syncfusion.Blazor;

// Get Azure credentials
var userAssignedClientId = Environment.GetEnvironmentVariable("CLIENT_ID");
var azCredentials = new DefaultAzureCredential(
    new DefaultAzureCredentialOptions()
    {
        ManagedIdentityClientId = userAssignedClientId
    });

var builder = WebApplication.CreateBuilder(args);

// Get Application config
builder.Configuration.AddAzureAppConfiguration(options =>
{
    var endpoint = Environment.GetEnvironmentVariable("APP_CONFIGURATION_ENDPOINT");
    if (endpoint == null)
    {
        throw new ApplicationException("Missing environment variable: APP_CONFIGURATION_ENDPOINT");
    }
    options.Connect(new Uri(endpoint), azCredentials);
});

builder.Services.Configure<Config>(builder.Configuration.GetSection("Greenhouse:Config"));

// Configure Database
builder.Services.AddDbContextFactory<MetricsContext>(options =>
{
    options.UseCosmos(
        builder.Configuration.GetSection("Greenhouse:Config:DatabaseEndpoint").Value!,
        azCredentials,
        builder.Configuration.GetSection("Greenhouse:Config:DatabaseName").Value!
        );
});

// Configure Syncfusion license
var syncfusionLicense = builder.Configuration.GetSection("Greenhouse:Config:SyncfusionLicense").Value!;
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncfusionLicense);

// Add services to the container.
builder.Services.AddSyncfusionBlazor();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddScoped<IGreenhouseMetricService, GreenhouseMetricService>();
builder.Services.AddControllersWithViews();

// Add Authentication 
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration)
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddInMemoryTokenCaches();

builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();