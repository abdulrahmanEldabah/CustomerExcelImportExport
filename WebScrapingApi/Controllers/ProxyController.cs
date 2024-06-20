using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class ProxyController : ControllerBase
{
    private readonly IHttpClientFactory _clientFactory;

    public ProxyController(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    [HttpGet]
    public async Task<IActionResult> Get(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return BadRequest("URL parameter is required.");
        }

        try
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, response.ReasonPhrase);
            }

            var content = await response.Content.ReadAsStringAsync();
            return Ok(content);
        }
        catch (HttpRequestException ex)
        {
            return BadRequest($"Error fetching data from {url}: {ex.Message}");
        }
    }
}
