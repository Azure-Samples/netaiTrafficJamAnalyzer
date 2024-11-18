using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace TrafficJamAnalyzer.Shared.Clients
{
    public class ScrapApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ScrapApiClient> _logger;

        public ScrapApiClient(HttpClient httpClient, ILogger<ScrapApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<string>> ScrapAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Sending scrap request.");

            var response = await _httpClient.GetAsync("/scrap", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to scrap traffic cameras.");
                return new();
            }

            var imageUrls = await response.Content.ReadFromJsonAsync<List<string>>(cancellationToken: cancellationToken);

            if (imageUrls == null)
            {
                _logger.LogWarning("No image URLs received from traffic camera service.");
                return new();
            }

            _logger.LogInformation("Received {Count} image URLs.", imageUrls.Count);
            return imageUrls;
        }
    }
}