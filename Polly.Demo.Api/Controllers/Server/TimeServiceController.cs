using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace Polly.Demo.Api.Controllers.Server;

[ApiController]
[Route("[controller]")]
public class TimeServiceController : ControllerBase
{
    private readonly ILogger<TimeServiceController> _logger;

    public TimeServiceController(ILogger<TimeServiceController> logger)
    {
        _logger = logger;
    }

    [HttpGet("{delayMilliseconds:int?}")]
    public async Task<IActionResult> Get(int delayMilliseconds = 500, CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(delayMilliseconds, cancellationToken);
            return Ok($"The time now is: {DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Exception occurred:");
            return StatusCode(StatusCodes.Status500InternalServerError, exception);
        }
    }
}