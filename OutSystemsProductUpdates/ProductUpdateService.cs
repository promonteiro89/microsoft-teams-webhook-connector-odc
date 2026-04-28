using System.Text;
using System.Net.Http.Headers;
using HtmlAgilityPack;
using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.OSProductUpdatesProvider
{
    /// <summary>
    /// Service to extract product updates from a target web page.
    /// Optimized for high-performance with stream-based parsing and minimal memory allocations.
    /// </summary>
    public sealed class OSProductUpdatesProvider : IOSProductUpdatesProvider
    {
        // Thread-safe, high-performance HttpClient with optimized connection pooling
        private static readonly HttpClient _httpClient = new(new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            MaxConnectionsPerServer = 20,
            EnableMultipleHttp2Connections = true
        });

        static OSProductUpdatesProvider()
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public IEnumerable<ProductUpdate> GetOSProductUpdatesByDate(DateTime targetDate, string productUpdatesUrl)
        {
            if (string.IsNullOrWhiteSpace(productUpdatesUrl))
            {
                throw new ArgumentException("Source URL cannot be empty.", nameof(productUpdatesUrl));
            }

            return FetchUpdatesAsync(targetDate, productUpdatesUrl).GetAwaiter().GetResult();
        }

        private async Task<List<ProductUpdate>> FetchUpdatesAsync(DateTime targetDate, string targetUrl)
        {
            try
            {
                var uri = new Uri(targetUrl);
                string baseUrl = $"https://{uri.Host}";

                using var stream = await _httpClient.GetStreamAsync(targetUrl).ConfigureAwait(false);
                var doc = new HtmlDocument();
                doc.Load(stream);

                var articleNodes = doc.DocumentNode.SelectNodes("//article[contains(@class, 'os-card02')]");
                if (articleNodes == null) return [];

                var resultList = new List<ProductUpdate>(articleNodes.Count);

                foreach (var node in articleNodes)
                {
                    var timeNode = node.SelectSingleNode(".//time[@datetime]");
                    if (timeNode == null || !DateTime.TryParse(timeNode.GetAttributeValue("datetime", ""), out var articleDate))
                        continue;

                    if (articleDate.Date != targetDate.Date) continue;

                    var contentBuilder = new StringBuilder(512);
                    var collapseDiv = node.SelectSingleNode(".//div[contains(@class, 'os-card02__collapse')]");

                    if (collapseDiv != null)
                    {
                        foreach (var child in collapseDiv.DescendantsAndSelf())
                        {
                            if (child.Name == "p")
                            {
                                // Skip <p> tags that are inside a <li> — they'll be handled by the li branch
                                if (child.Ancestors("li").Any()) continue;

                                var text = HtmlEntity.DeEntitize(child.InnerText).Trim();
                                if (string.IsNullOrWhiteSpace(text)) continue;

                                contentBuilder.AppendLine(text);
                                contentBuilder.AppendLine();
                            }
                            else if (child.Name == "li")
                            {
                                var text = HtmlEntity.DeEntitize(child.InnerText).Trim();
                                if (string.IsNullOrWhiteSpace(text)) continue;

                                contentBuilder.AppendLine($"* {text}");
                            }
                        }
                    }

                    var titleNode = node.SelectSingleNode(".//h5");
                    var imgNode = node.SelectSingleNode(".//img[@data-src]") ?? node.SelectSingleNode(".//img[@src]");
                    var linkNode = node.SelectSingleNode(".//a[contains(@class, 'product-card__link')]");

                    string imgPath = HtmlEntity.DeEntitize(imgNode?.GetAttributeValue("data-src", "") ?? "");
                    if (string.IsNullOrEmpty(imgPath)) imgPath = HtmlEntity.DeEntitize(imgNode?.GetAttributeValue("src", "") ?? "");
                    
                    string relativeLink = HtmlEntity.DeEntitize(linkNode?.GetAttributeValue("href", "") ?? "");

                    resultList.Add(new ProductUpdate
                    {
                        Title = titleNode?.InnerText?.Trim() ?? "Untitled",
                        Content = contentBuilder.ToString().Trim(),
                        ImageUrl = NormalizeUrl(baseUrl, imgPath),
                        Url = NormalizeUrl(baseUrl, relativeLink),
                        PublishDate = articleDate
                    });
                }

                return resultList;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Network error: {ex.StatusCode}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Processing failure: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Robust URL normalization ensuring HTTPS and proper path joining.
        /// </summary>
        private static string NormalizeUrl(string baseUrl, string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;

            // Handle Protocol-relative (e.g. //assets.example.com)
            if (path.StartsWith("//")) return $"https:{path}";

            // Handle Absolute URLs
            if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                // Force HTTPS for service compatibility
                if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                    return "https://" + path.Substring(7);
                return path;
            }

            // Handle Absolute path on same domain
            if (path.StartsWith("/")) return $"{baseUrl}{path}";

            // Handle Relative path
            return $"{baseUrl}/{path}";
        }
    }
}
