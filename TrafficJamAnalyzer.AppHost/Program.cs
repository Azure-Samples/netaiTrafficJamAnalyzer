
var builder = DistributedApplication.CreateBuilder(args);

var chatDeploymentName = "chat";
var aoai = builder.AddAzureOpenAI("openai")
    .AddDeployment(new AzureOpenAIDeployment(chatDeploymentName,
    "gpt-4o", //"gpt-4o-mini", 
    "2024-05-13", //"2024-07-18", 
    "GlobalStandard", 
    10));

var sqldb = builder.AddSqlServer("sql")
    .WithDataVolume()
    .AddDatabase("sqldb");

var apiService = builder.AddProject<Projects.TrafficJamAnalyzer_Microservices_WebApiService>("apiservice")
    .WithReference(sqldb);

var aiService = builder.AddProject<Projects.TrafficJamAnalyzer_Microservices_AiApiService>("aiservice")
    .WithReference(aoai)
    .WithEnvironment("AI_ChatDeploymentName", chatDeploymentName); ;

var scrapService = builder.AddProject<Projects.TrafficJamAnalyzer_Microservices_ScraperApiService>("scrapservice");

var worker = builder.AddProject<Projects.TrafficJamAnalyzer_Workers_Analyzer>("worker")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WithReference(aiService)
    .WithReference(scrapService);


builder.AddProject<Projects.TrafficJamAnalyzer_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WithReference(scrapService);

builder.Build().Run();
