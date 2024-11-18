using System.Net.Http.Json;
using TrafficJamAnalyzer.Shared.Models;

namespace TrafficJamAnalyzer.Shared.Clients
{
    public class WebApiClient
    {
        private readonly HttpClient _httpClient;

        public WebApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TrafficEntry[]> GetTrafficAsync(CancellationToken cancellationToken = default)
        {
            List<TrafficEntry>? traffics = null;

            await foreach (var traffic in _httpClient.GetFromJsonAsAsyncEnumerable<TrafficEntry>("/traffics", cancellationToken))
            {
                if (traffic is not null)
                {
                    traffics ??= new List<TrafficEntry>();

                    var results = await GetTrafficResultsAsync(traffic.Id, cancellationToken);

                    traffic.Results = results.ToList();

                    traffics.Add(traffic);
                }
            }

            return traffics?.ToArray() ?? Array.Empty<TrafficEntry>();
        }

        public async Task<TrafficResult[]> GetTrafficResultsAsync(int id, CancellationToken cancellationToken = default)
        {
            List<TrafficResult>? traffics = null;

            await foreach (var traffic in _httpClient.GetFromJsonAsAsyncEnumerable<TrafficResult>($"/traffics/{id}/results", cancellationToken))
            {
                if (traffic is not null)
                {
                    traffics ??= new List<TrafficResult>();
                    traffics.Add(traffic);
                }
            }

            return traffics?.ToArray() ?? Array.Empty<TrafficResult>();
        }

        public async Task<bool> ToggleTrafficAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PutAsync($"/toggle-traffic/{id}", null!, cancellationToken);
            return response.IsSuccessStatusCode;
        }

        public async Task<TrafficResult?> AddTrafficResultAsync(int id, TrafficResult trafficResult, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync($"/traffic/{id}/results", trafficResult, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TrafficResult>(cancellationToken: cancellationToken);
            }
            return null;
        }

        public async Task<TrafficEntry?> UpdateTrafficAsync(int id, TrafficEntry trafficEntry, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync($"/traffic/{id}/title", trafficEntry, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TrafficEntry>(cancellationToken: cancellationToken);
            }
            return null;
        }

        public async Task<TrafficEntry?> AddTrafficAsync(TrafficEntry trafficEntry, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync($"/traffic", trafficEntry, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TrafficEntry>(cancellationToken: cancellationToken);
            }
            return null;
        }
    }
}