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
        _httpClient = httpClientFactory.CreateClient("TimeService");
        _tokenSource = new CancellationTokenSource();
    }

    [HttpGet("{delayMilliseconds:int?}")]
    public async Task<IActionResult> Get(int delayMilliseconds = 500)
    {
        try
        {
            var uri = $"https://localhost:7018/TimeService/{delayMilliseconds}";
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