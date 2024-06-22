using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HttpMethod = System.Net.Http.HttpMethod;

namespace WebScrapingApi
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProxyController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly DatabaseService _databaseService;

        public ProxyController(HttpClient httpClient, DatabaseService databaseService)
        {
            _httpClient = httpClient;
            _databaseService = databaseService;
        }

        [HttpGet("fetchLinks")]
        public async Task<IActionResult> FetchLinks([FromQuery] string url, [FromQuery] int maxDepth = 2, [FromQuery] int maxMinutes = 1, [FromQuery] string filterText = "")
        {
            if (string.IsNullOrEmpty(url))
            {
                return BadRequest("URL is required");
            }

            var cts = new CancellationTokenSource(TimeSpan.FromMinutes(maxMinutes));
            var token = cts.Token;
            var scrapedDataList = new List<ScrapedData>();

            try
            {
                await FetchAndSaveLinksRecursive(url, 0, maxDepth, token, scrapedDataList, filterText);
                return Ok(scrapedDataList);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(408, "Request timed out");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private async Task FetchAndSaveLinksRecursive(string url, int depth, int maxDepth, CancellationToken token, List<ScrapedData> scrapedDataList, string filterText)
        {
            if (depth > maxDepth || scrapedDataList.Any(data => data.Url == url) || token.IsCancellationRequested)
            {
                return; // Stop recursion if maximum depth is reached, link already processed, or cancellation requested
            }

            if (!IsValidHttpUrl(url))
            {
                return; // Skip non-HTTP/HTTPS URLs
            }

            try
            {
                var html = await FetchHtmlAsync(url);
                var document = await ParseHtmlAsync(html);

                var visibleText = ExtractVisibleText(document);

                if (string.IsNullOrEmpty(filterText) || visibleText.Contains(filterText, StringComparison.OrdinalIgnoreCase))
                {
                    var emails = ExtractEmails(visibleText);
                    var phoneNumbers = ExtractPhoneNumbers(visibleText);

                    scrapedDataList.Add(new ScrapedData
                    {
                        Url = url,
                        Emails = emails,
                        PhoneNumbers = phoneNumbers
                    });

                    var extractedLinks = ExtractLinks(document);

                    foreach (var link in extractedLinks)
                    {
                        if (token.IsCancellationRequested) break; // Stop if cancellation is requested

                        string fullLink = ConstructLink(link, url);
                        await FetchAndSaveLinksRecursive(fullLink, depth + 1, maxDepth, token, scrapedDataList, filterText);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fetch HTML from {url}: {ex.Message}");
            }
        }

        private bool IsValidHttpUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        private string ConstructLink(string link, string baseUrl)
        {
            // Check if link is a complete URL or a relative path
            if (!Uri.TryCreate(link, UriKind.Absolute, out Uri uri))
            {
                // Combine baseUrl with the relative link
                uri = new Uri(new Uri(baseUrl), link);
            }

            return uri.ToString();
        }

        public async Task<string> FetchHtmlAsync(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
            request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            request.Headers.Add("DNT", "1"); // Do Not Track request header

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<IDocument> ParseHtmlAsync(string html)
        {
            var context = BrowsingContext.New(Configuration.Default.WithDefaultLoader(new LoaderOptions
            {
                IsResourceLoadingEnabled = false // Disable loading of additional resources like images, CSS, JS
            }));
            return await context.OpenAsync(req => req.Content(html));
        }

        public string ExtractVisibleText(IDocument document)
        {
            return document.Body.TextContent;
        }

        public List<string> ExtractLinks(IDocument document)
        {
            return document.QuerySelectorAll("a[href]")
                .Select(element => element.GetAttribute("href"))
                .ToList();
        }

        public List<string> ExtractEmails(string text)
        {
            var emailPattern = @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}";
            var matches = Regex.Matches(text, emailPattern);
            return matches.Select(m => m.Value).ToList();
        }

        public List<string> ExtractPhoneNumbers(string text)
        {
            var phonePattern = @"\+?\d{1,4}?[-.\s]?(\(?\d{1,3}?\)?[-.\s]?)?\d{1,4}[-.\s]?\d{1,4}[-.\s]?\d{1,9}";
            var matches = Regex.Matches(text, phonePattern);

            var filteredNumbers = matches
                .Select(m => m.Value)
                .Where(number => number.Length >= 9 && number.Length <= 12)
                .ToList();

            return filteredNumbers;
        }

       
    }
}
