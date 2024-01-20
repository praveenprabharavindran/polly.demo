using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Polly.Demo.Api.Controllers.Consumer;

[ApiController]
[Route("[controller]")]
public class ResilientConsumerController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ResilientConsumerController> _logger;
    private readonly CancellationTokenSource _tokenSource;

    public ResilientConsumerController(ILogger<ResilientConsumerController> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("ChaoticService");
        _tokenSource = new CancellationTokenSource();
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            var uri = $"https://localhost:7018/ChaoticService";
            var response = await _httpClient.GetAsync(uri, _tokenSource.Token);
            response.EnsureSuccessStatusCode();
            return Ok(await response.Content.ReadAsStringAsync());
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Exception occurred:");
            return StatusCode(StatusCodes.Status500InternalServerError, exception.ToString());
        }
    }
}