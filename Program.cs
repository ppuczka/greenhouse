using Azure.Identity;
using Greenhouse.Components;
using Greenhouse.Config;
using Greenhouse.Data;
using Greenhouse.Data.Interfaces;
using Greenhouse.Data.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var azCredentials = new DefaultAzureCredential();

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Configuration.AddAzureAppConfiguration(options =>
{
    var endpoint = Environment.GetEnvironmentVariable("APP_CONFIGURATION_ENDPOINT");
    options.Connect(new Uri(endpoint ?? string.Empty), azCredentials);
});

builder.Services.Configure<Config>(builder.Configuration.GetSection("Greenhouse:Config"));
builder.Services.AddDbContextFactory<MetricsContext>(options =>
{
    options.UseCosmos(
        builder.Configuration.GetSection("Greenhouse:Config:DatabaseEndpoint").Value!,
        azCredentials,
        builder.Configuration.GetSection("Greenhouse:Config:DatabaseName").Value!
        );
});

builder.Services.AddScoped<IGreenhouseMetricService, GreenhouseMetricService>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();