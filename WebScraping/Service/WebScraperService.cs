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
        var apiUrl = $"api/proxy?url={Uri.EscapeDataString(url)}"; // Endpoint of your ProxyController
        var response = await _httpClient.GetAsync(apiUrl);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
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
