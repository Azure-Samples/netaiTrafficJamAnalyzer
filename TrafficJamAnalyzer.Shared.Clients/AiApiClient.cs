using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Json;
using TrafficJamAnalyzer.Shared.Models;

namespace TrafficJamAnalyzer.Shared.Clients
{
    public class AiApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AiApiClient> _logger;

        public AiApiClient(HttpClient httpClient, ILogger<AiApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<TrafficJamAnalyzeResult?> AnalyzeAsync(string identifier, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Sending analyze request with identifier: {Identifier}", identifier);

            var response = await _httpClient.GetAsync($"/analyze/{identifier}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to analyze traffic jam with identifier: {Identifier}", identifier);
                return null;
            }

            var analyzeResult = await response.Content.ReadFromJsonAsync<TrafficJamAnalyzeResult>(cancellationToken: cancellationToken);

            if (analyzeResult == null)
            {
                _logger.LogWarning("No content received from AI API service.");
                return null;
            }

            _logger.LogInformation("Analysis result received: {AnalyzeResult}", JsonConvert.SerializeObject(analyzeResult));

            return analyzeResult;
        }
    }
}