using TrafficJamAnalyzer.Services.TrafficService;

var builder = WebApplication.CreateBuilder(args);

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddHttpClient();
builder.Services.AddScoped<TrafficCameraService>();

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

var app = builder.Build();
var logger = app.Logger;

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

// Map the endpoint with logging
app.MapGet("/scrap", async (TrafficCameraService trafficCameraService, ILogger<Program> logger) =>
{
    logger.LogInformation("Scrap endpoint called.");

    const string trafficCamerasUrl = "https://cic.tenerife.es/web3/mosaico_cctv/camaras_trafico_b.html";
    logger.LogInformation("Fetching HTML content from URL: {TrafficCamerasUrl}", trafficCamerasUrl);

    var htmlContent = await trafficCameraService.GetHtmlContentAsync(trafficCamerasUrl);
    logger.LogInformation("HTML content received. Length: {Length}", htmlContent.Length);

    var imageUrls = await trafficCameraService.GetTrafficCameraImageUrlsAsync(htmlContent);
    logger.LogInformation("Extracted {Count} image URLs.", imageUrls.Count);

    return imageUrls;
});

logger.LogInformation("Application starting up.");
app.Run();
logger.LogInformation("Application shut down.");