namespace WebScrapingApi;

using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> FetchLinks([FromQuery] string url, [FromQuery] int maxDepth = 2, [FromQuery] int maxMinutes = 1)
    {
        if (string.IsNullOrEmpty(url))
        {
            return BadRequest("URL is required");
        }

        var cts = new CancellationTokenSource(TimeSpan.FromMinutes(maxMinutes));
        var token = cts.Token;
        var links = new HashSet<string>();

        try
        {
            await FetchAndSaveLinksRecursive(url, 0, maxDepth, token, links);
            return Ok(links);
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

    private async Task FetchAndSaveLinksRecursive(string url, int depth, int maxDepth, CancellationToken token, HashSet<string> links)
    {
        if (depth > maxDepth || links.Contains(url) || token.IsCancellationRequested)
        {
            return; // Stop recursion if maximum depth is reached, link already processed, or cancellation requested
        }

        if (!IsValidHttpUrl(url))
        {
            return; // Skip non-HTTP/HTTPS URLs
        }

        try
        {
            links.Add(url);
            _databaseService.SaveLink(url);

            var html = await FetchHtmlAsync(url);
            var document = ParseHtml(html);
            var extractedLinks = ExtractLinks(document);

            foreach (var link in extractedLinks)
            {
                if (token.IsCancellationRequested) break; // Stop if cancellation is requested

                string fullLink = ConstructLink(link, url);
                await FetchAndSaveLinksRecursive(fullLink, depth + 1, maxDepth, token, links);
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

    public HtmlDocument ParseHtml(string html)
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);
        return document;
    }

    public List<string> ExtractLinks(HtmlDocument document)
    {
        return document.DocumentNode.SelectNodes("//a[@href]")
            .Select(node => node.Attributes["href"].Value)
            .ToList();
    }
}
