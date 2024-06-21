using AngleSharp;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

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

                if (string.IsNullOrEmpty(filterText) || document.DocumentElement.TextContent.Contains(filterText, StringComparison.OrdinalIgnoreCase))
                {
                    var emails = ExtractEmails(html);
                    var phoneNumbers = ExtractPhoneNumbers(html);

                    scrapedDataList.Add(new ScrapedData
                    {
                        Url = url,
                        Emails = emails,
                        PhoneNumbers = phoneNumbers
                    });

                    _databaseService.SaveLink(url);

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
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<IDocument> ParseHtmlAsync(string html)
        {
            var context = BrowsingContext.New(Configuration.Default);
            return await context.OpenAsync(req => req.Content(html));
        }

        public List<string> ExtractLinks(IDocument document)
        {
            return document.QuerySelectorAll("a[href]")
                .Select(element => element.GetAttribute("href"))
                .ToList();
        }

        public List<string> ExtractEmails(string html)
        {
            var emailPattern = @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}";
            var matches = Regex.Matches(html, emailPattern);
            return matches.Select(m => m.Value).ToList();
        }

        public List<string> ExtractPhoneNumbers(string html)
        {
            var phonePattern = @"\+?\d{1,4}?[-.\s]?(\(?\d{1,3}?\)?[-.\s]?)?\d{1,4}[-.\s]?\d{1,4}[-.\s]?\d{1,9}";
            var matches = Regex.Matches(html, phonePattern);
            return matches.Select(m => m.Value).ToList();
        }
    }
}
