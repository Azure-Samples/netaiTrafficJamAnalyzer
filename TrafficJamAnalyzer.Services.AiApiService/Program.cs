using Newtonsoft.Json;
using TrafficJamAnalyzer.Shared.Models;
using OpenAI;
using OpenAI.Chat;

// Builder
var builder = WebApplication.CreateBuilder(args);
var kernelBuilder = Kernel.CreateBuilder();

var prompt = builder.Configuration["OpenAI:Prompt"];
var systemPrompt = "You are a useful assistant that replies using a direct style";

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Add OpenAI
var openAiClientName = builder.Environment.IsDevelopment() ? "openaidev" : "openai";
builder.AddAzureOpenAIClient(openAiClientName);

// get chat client from aspire hosting configuration
builder.Services.AddSingleton(serviceProvider =>
{
    var config = serviceProvider.GetService<IConfiguration>()!;
    var logger = serviceProvider.GetService<ILogger<Program>>()!;
    logger.LogInformation($"Chat client configuration, modelId: {config["AI_ChatDeploymentName"]}");
    OpenAIClient client = serviceProvider.GetRequiredService<OpenAIClient>();
    var chatClient = client.GetChatClient(config["AI_ChatDeploymentName"]);
    return chatClient;
});

var app = builder.Build();
var logger = app.Logger;
logger.LogInformation("Application starting up.");
logger.LogInformation($"Azure OpenAI Client using: {openAiClientName}");
var kernel = kernelBuilder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

// Map the endpoint with logging
app.MapGet("/analyze/{identifier}", async (string identifier, ILogger<Program> logger, ChatClient client) =>
{
    logger.LogInformation("Received analyze request with identifier: {Identifier}", identifier);

    var imageUrl = $"http://cic.tenerife.es/e-Traffic3/data/{identifier}.jpg";

    // "Prompt": "The image I'm going to provide you is an image from a CCTV that shows a road, I need you to give me a JSON object with 'Title' which is title in the top left and 'Traffic' which is an integer from 0 to 100 which shows the amount of traffic jam and the 'Date' that is on the bottom right, please only provide the JSON result and nothing else. Return only the json object without any markdown. If you a lot of lanes, please focus on the one that is busy when checking for the traffic, so, if you see 4 lanes and only 2 are full, it means that the traffic is jammed."

    var imageContentPart = ChatMessageContentPart.CreateImagePart(imageUri: new Uri(imageUrl));

    var messages = new List<ChatMessage>
    {
        new SystemChatMessage(systemPrompt),
        new UserChatMessage(prompt),
        new UserChatMessage(imageContentPart)
    };

    logger.LogInformation("Image URL generated: {ImageUrl}", imageUrl);
    logger.LogInformation("Chat history created: {ChatHistory}", JsonConvert.SerializeObject(messages));

    var result = await client.CompleteChatAsync(messages);

    var content = result.Value.Content[0].Text!;

    if (content == null)
    {
        logger.LogWarning("No content received from chatCompletionService.");
        return new TrafficJamAnalyzeResult();
    }

    logger.LogInformation("Content received: {Content}", content);

    var analyzeResult = new TrafficJamAnalyzeResult
    {
        CreatedAt = DateTime.UtcNow,
        Result = JsonConvert.DeserializeObject<TrafficJamAnalyze>(content)!,
        SourceUrl = imageUrl
    };

    logger.LogInformation("Analysis result created: {AnalyzeResult}", JsonConvert.SerializeObject(analyzeResult));

    return analyzeResult;
});

logger.LogInformation("Application starting up.");
app.Run();
logger.LogInformation("Application shut down.");