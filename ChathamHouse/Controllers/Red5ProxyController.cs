using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace ChathamHouse.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Red5ProxyController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<Red5ProxyController> _logger;

        public Red5ProxyController(IHttpClientFactory httpClientFactory, ILogger<Red5ProxyController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet("whep")]
        public async Task ProxyWhepRequest([FromQuery] string streamName)
        {
            try
            {
                if (string.IsNullOrEmpty(streamName))
                {
                    _logger.LogWarning("streamName parameter is missing");
                    Response.StatusCode = 400;
                    await Response.WriteAsync("streamName is required");
                    return;
                }

                // Your Red5 Cloud configuration
                var red5Host = "userId-2441-c10d446375.cloud.red5.net";
                var red5Url = $"https://{red5Host}/as/v1/proxy/whep/live/{streamName}";

                _logger.LogInformation($"Proxying request to: {red5Url}");

                var client = _httpClientFactory.CreateClient();

                // Forward all query parameters
                var queryString = Request.QueryString.Value;
                var fullUrl = red5Url + queryString;

                // Set a timeout
                client.Timeout = TimeSpan.FromSeconds(30);

                // Forward the request to Red5
                var response = await client.GetAsync(fullUrl);

                // Copy response status code
                Response.StatusCode = (int)response.StatusCode;

                // Copy response headers
                foreach (var header in response.Headers)
                {
                    Response.Headers[header.Key] = new StringValues(header.Value.ToArray());
                }

                // Copy content headers
                foreach (var header in response.Content.Headers)
                {
                    Response.Headers[header.Key] = new StringValues(header.Value.ToArray());
                }

                // Copy response body
                var content = await response.Content.ReadAsByteArrayAsync();
                await Response.Body.WriteAsync(content);

                _logger.LogInformation($"Proxy request completed with status: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in proxy request");
                Response.StatusCode = 500;
                await Response.WriteAsync($"Proxy error: {ex.Message}");
            }
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}