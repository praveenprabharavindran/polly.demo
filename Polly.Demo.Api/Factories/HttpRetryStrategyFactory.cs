using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly.Timeout;

namespace Polly.Demo.Api.Factories;

public class HttpRetryStrategyFactory : IHttpRetryStrategyFactory
{
    private readonly RetryConfig _config;
    private readonly ILogger<HttpRetryStrategyFactory> _logger;

    public HttpRetryStrategyFactory(ILogger<HttpRetryStrategyFactory> logger, IOptions<RetryConfig> config)
    {
        _logger = logger;
        _config = config.Value;
    }

    public ResiliencePipeline<HttpResponseMessage> Create()
    {
        var timeoutStrategyOptions = MapToTimeoutStrategyOptions(_config);
        var retryStrategyOptions = MapToHttpRetryStrategyOptions(_config);
        var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(retryStrategyOptions)
            .AddTimeout(timeoutStrategyOptions)
            .Build();
        return pipeline;
    }

    private TimeoutStrategyOptions MapToTimeoutStrategyOptions(RetryConfig config)
    {
        return new TimeoutStrategyOptions
        {
            Timeout = TimeSpan.FromSeconds(_config.PertAttemptTimeoutSeconds),
            OnTimeout = args =>
            {
                _logger.LogWarning(
                    $"{args.Context.OperationKey}: Execution timed out after {args.Timeout.TotalSeconds} seconds.");
                return default;
            }
        };
    }

    public HttpRetryStrategyOptions MapToHttpRetryStrategyOptions(RetryConfig config)
    {
        return new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = config.MaxRetryAttempts,
            Delay = TimeSpan.FromSeconds(config.RetryDelaySeconds),
            OnRetry = args =>
            {
                var msg =
                    $"{args.Context.OperationKey}: Retry {args.AttemptNumber} implemented with a delay of {args.RetryDelay} seconds, due to: {args.Outcome.Exception?.Message}";
                _logger.LogWarning(msg);
                return default;
            }
        };
    }
}