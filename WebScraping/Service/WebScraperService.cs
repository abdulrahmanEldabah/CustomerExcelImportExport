// Services/WebScraperService.cs
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Collections.Generic;
namespace WebScraping;
public class WebScraperService
{
    private readonly HttpClient _httpClient;

    public WebScraperService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> FetchHtmlAsync(string url)
    {
        try
        {
            var apiUrl = $"api/proxy?url={Uri.EscapeDataString(url)}"; // Endpoint of your ProxyController
            Console.WriteLine($"Fetching HTML from: {apiUrl}"); // Log the constructed URL for debugging
            var response = await _httpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Fetched HTML content length: {content.Length}"); // Log the length of the fetched content
            return content;
        }
        catch (HttpRequestException httpEx)
        {
            Console.WriteLine($"HTTP Request error: {httpEx.Message}"); // Log HTTP-specific errors
            return string.Empty;
           
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General error: {ex.Message}"); // Log general errors
            return string.Empty;
        }
    }


    public HtmlDocument ParseHtml(string htmlContent)
    {
        var document = new HtmlDocument();
        document.LoadHtml(htmlContent);
        return document;
    }

    public List<string> ExtractLinks(HtmlDocument document)
    {
        var links = new List<string>();
        foreach (var link in document.DocumentNode.SelectNodes("//a[@href]"))
        {
            var href = link.GetAttributeValue("href", string.Empty);
            if (!string.IsNullOrEmpty(href))
            {
                links.Add(href);
            }
        }
        return links;
    }
}
