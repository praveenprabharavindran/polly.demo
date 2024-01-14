using Microsoft.Extensions.Options;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace Polly.Demo.Api.Factories;

public class HttpRetryStrategyFactory : IHttpRetryStrategyFactory
{
    private readonly ILogger<HttpRetryStrategyFactory> _logger;
    private readonly RetryConfig _config;

    public HttpRetryStrategyFactory(ILogger<HttpRetryStrategyFactory> logger, IOptions<RetryConfig> config)
    {
        _logger = logger;
        _config = config.Value;
    }    
    public IAsyncPolicy<HttpResponseMessage> Create()
    {
        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(_config.PertAttemptTimeoutSeconds);

        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(_config.MaxRetryAttempts, retryAttempt => TimeSpan.FromSeconds(_config.RetryDelaySeconds));
        return Policy.WrapAsync(retryPolicy, timeoutPolicy);
    }
}

public class RetryConfig
{
    // PertAttemptTimeoutSeconds is the timeout for each attempt in seconds
    public int PertAttemptTimeoutSeconds { get; set; }

    // MaxRetryAttempts is the number of retries before giving up
    public int MaxRetryAttempts { get; set; }
    
    // RetryDelaySeconds is the median delay between retries in seconds
    public int RetryDelaySeconds { get; set; }
}
public interface IHttpRetryStrategyFactory
{
    IAsyncPolicy<HttpResponseMessage> Create();
}