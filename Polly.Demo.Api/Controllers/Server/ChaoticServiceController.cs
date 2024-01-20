using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace Polly.Demo.Api.Controllers.Server;

[ApiController]
[Route("[controller]")]
public class ChaoticServiceController : ControllerBase
{
    private readonly ILogger<ChaoticServiceController> _logger;
    private readonly Random _random = new Random();
    private const int DelayMilliseconds = 3000;

    public ChaoticServiceController(ILogger<ChaoticServiceController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken = default)
    {
        try
        {
            // 50% chance to delay
            if (_random.Next(2) == 0) // Generates 0 or 1 randomly
            {
                await Task.Delay(DelayMilliseconds, cancellationToken);
            }
            return Ok($"The time now is: {DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Exception occurred:");
            return StatusCode(StatusCodes.Status500InternalServerError, exception);
        }
    }
}