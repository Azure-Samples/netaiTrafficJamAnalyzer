using Microsoft.Extensions.Logging;

namespace TrafficJamAnalyzer.Services.TrafficService
{
    public class TrafficCameraService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TrafficCameraService> _logger;

        public TrafficCameraService(HttpClient httpClient, ILogger<TrafficCameraService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> GetHtmlContentAsync(string url)
        {
            _logger.LogInformation("Fetching HTML content from URL: {Url}", url);
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Fetched HTML content. Length: {Length}", content.Length);
            return content;
        }

        public List<string> GetIframeSources(string htmlContent)
        {
            if (string.IsNullOrEmpty(htmlContent))
            {
                _logger.LogWarning("HTML content is null or empty.");
                throw new ArgumentException("HTML content cannot be null or empty.", nameof(htmlContent));
            }

            var iframeSources = new List<string>();
            var iframeSplits = htmlContent.Split(new string[] { "<iframe" }, StringSplitOptions.None);

            _logger.LogInformation("Extracting iframe sources from HTML content.");

            for (int i = 1; i < iframeSplits.Length; i++)
            {
                var part = iframeSplits[i];
                var srcIndex = part.IndexOf("src=\"", StringComparison.InvariantCultureIgnoreCase);
                if (srcIndex != -1)
                {
                    var srcStart = srcIndex + 5;
                    var srcEnd = part.IndexOf("\"", srcStart, StringComparison.InvariantCultureIgnoreCase);

                    if (srcEnd != -1)
                    {
                        var srcValue = part.Substring(srcStart, srcEnd - srcStart);
                        iframeSources.Add(srcValue);
                    }
                }
            }

            _logger.LogInformation("Extracted {Count} iframe sources.", iframeSources.Count);

            return iframeSources;
        }

        private string ExtractImageSource(string iframeData)
        {
            if (string.IsNullOrEmpty(iframeData))
            {
                _logger.LogWarning("Iframe data is null or empty.");
                throw new ArgumentException("Iframe data cannot be null or empty.", nameof(iframeData));
            }

            _logger.LogInformation("Extracting image source from iframe data.");

            var imageSplit = iframeData.Split(new string[] { "imgsrc = \"" }, StringSplitOptions.None);
            if (imageSplit.Length > 1)
            {
                var imageSrc = imageSplit[1].Split(new string[] { "\"" }, StringSplitOptions.None)[0];
                _logger.LogInformation("Extracted image source: {ImageSrc}", imageSrc);
                return imageSrc;
            }

            _logger.LogWarning("No image source found in iframe data.");
            return string.Empty;
        }

        public async Task<List<string>> GetTrafficCameraImageUrlsAsync(string htmlContent)
        {
            if (string.IsNullOrEmpty(htmlContent))
            {
                _logger.LogWarning("HTML content is null or empty.");
                throw new ArgumentException("HTML content cannot be null or empty.", nameof(htmlContent));
            }

            var imageUrls = new List<string>();
            var iframes = GetIframeSources(htmlContent);

            _logger.LogInformation("Fetching traffic camera image URLs from iframes.");

            foreach (var iframeSource in iframes)
            {
                try
                {
                    var iframeUrl = iframeSource.Replace("..", "https://cic.tenerife.es/web3");
                    _logger.LogInformation("Fetching iframe data from URL: {IframeUrl}", iframeUrl);
                    var iframeData = await _httpClient.GetStringAsync(iframeUrl);
                    var iframeSrcImage = ExtractImageSource(iframeData);

                    if (!string.IsNullOrEmpty(iframeSrcImage))
                    {
                        imageUrls.Add(iframeSrcImage);
                        _logger.LogInformation("Added image URL: {ImageUrl}", iframeSrcImage);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing iframe source {IframeSource}", iframeSource);
                }
            }

            _logger.LogInformation("Total extracted image URLs: {Count}", imageUrls.Count);

            return imageUrls;
        }
    }
}