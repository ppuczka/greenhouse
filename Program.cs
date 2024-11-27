using Azure.Identity;
using Greenhouse.Components;
using Greenhouse.Config;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Configuration.AddAzureAppConfiguration(options =>
{
    var endpoint = builder.Configuration.GetSection("Endpoints:AppConfiguration")
        .Value ?? throw new ArgumentNullException("Endpoints:AppConfiguration");
    
    var azCredentials = new DefaultAzureCredential();
    options.Connect(new Uri(endpoint), azCredentials);
});

builder.Services.Configure<AppConfig>(builder.Configuration.GetSection("Config"));


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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();