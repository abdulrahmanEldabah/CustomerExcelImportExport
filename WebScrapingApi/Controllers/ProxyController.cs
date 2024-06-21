using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace WebScrapingApi;

[ApiController]
[Route("api/[controller]")]
public class ProxyController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProxyController> _logger;

    public ProxyController(HttpClient httpClient, ILogger<ProxyController> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return BadRequest("URL is required.");
        }

        try
        {
            _logger.LogInformation($"Fetching URL: {url}");
            var response = await _httpClient.GetAsync(url);
            _logger.LogInformation($"Response Status: {response.StatusCode}");

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"Content Length: {content.Length}");

            return Content(content, "text/html");
        }
        catch (HttpRequestException e)
        {
            _logger.LogError($"Error fetching URL: {e.Message}");
            return StatusCode(500, $"Error fetching URL: {e.Message}");
        }
    }
}
