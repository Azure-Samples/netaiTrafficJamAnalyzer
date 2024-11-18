using Microsoft.EntityFrameworkCore;
using TrafficJamAnalyzer.Microservices.WebApiService.Models;
using TrafficJamAnalyzer.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

// Add DbContext service
builder.AddSqlServerDbContext<Context>("sqldb");

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

// Combined endpoint to get traffics with their results
app.MapGet("/traffics", async (Context context) =>
{
    var traffics = await context.Traffics
                               .ToListAsync();
    return Results.Ok(traffics);
});

// Combined endpoint to get traffics with their results
app.MapGet("/traffics/{id}/results", async (Context context, int id) =>
{
    var traffics = await context.TrafficResults
        .Where(x => x.TrafficId == id)
        .ToListAsync();

    return Results.Ok(traffics);
});


app.MapPut("/toggle-traffic/{id}", async (Context context, int id) =>
{
    var traffic = await context.Traffics.FindAsync(id);
    if (traffic == null)
        return Results.NotFound();

    traffic.Enabled = !traffic.Enabled;
    context.Traffics.Update(traffic);
    await context.SaveChangesAsync();
    return Results.NoContent();
});

// Endpoint to add a single TrafficEntry
app.MapPost("/traffic", async (Context context, TrafficEntry trafficEntry) =>
{
    trafficEntry.CreatedAt = DateTime.UtcNow;
    trafficEntry.UpdatedAt = DateTime.UtcNow;

    context.Traffics.Add(trafficEntry);
    await context.SaveChangesAsync();

    return Results.Created($"/traffic/{trafficEntry.Id}", trafficEntry);
});

// Endpoint to add a single TrafficEntry
app.MapPost("/traffic/{id}/title", async (Context context, int id, TrafficEntry trafficEntry) =>
{
    var traffic = await context.Traffics.FindAsync(id);
    if (traffic == null)
        return Results.NotFound();

    traffic.UpdatedAt = DateTime.UtcNow;
    traffic.Title = trafficEntry.Title;

    await context.SaveChangesAsync();

    return Results.Created($"/traffic/{trafficEntry.Id}", trafficEntry);
});

// Endpoint to add TrafficResults to a TrafficEntry
app.MapPost("/traffic/{id}/results", async (Context context, int id, TrafficResult trafficResult) =>
{
    var traffic = await context.Traffics.FindAsync(id);
    if (traffic == null)
        return Results.NotFound();

    trafficResult.TrafficId = traffic.Id;
    trafficResult.CreatedAt = DateTime.Now.ToString("o"); // Set created date to current date-time

    traffic.Title = trafficResult.TrafficTitle;

    context.TrafficResults.Add(trafficResult);
    await context.SaveChangesAsync();

    return Results.Created($"/traffic/{id}/results/{trafficResult.Id}", trafficResult);
});

// Combined endpoint to get traffics with their results
app.MapGet("/exceptions", async (Context context) =>
{
    throw new Exception("Exception test!!!");
});

app.MapDefaultEndpoints();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Context>();
    //context.Database.EnsureDeleted();
    context.Database.EnsureCreated();
}

app.Run();