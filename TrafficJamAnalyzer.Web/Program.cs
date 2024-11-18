using TrafficJamAnalyzer.Shared.Clients;
using TrafficJamAnalyzer.Web.Components;
using TrafficJamAnalyzer.Web.Models;

var builder = WebApplication.CreateBuilder(args);

builder.AddSqlServerDbContext<Context>("sql");

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

builder.Services.AddHttpClient<AiApiClient>(client =>
    {
        client.BaseAddress = new("https+http://aiservice");
    });

builder.Services.AddHttpClient<WebApiClient>(client =>
{
    client.BaseAddress = new("https+http://apiservice");
});

builder.Services.AddHttpClient<ScrapApiClient>(client =>
{
    client.BaseAddress = new("https+http://scrapservice");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseOutputCache();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();


app.Run();
