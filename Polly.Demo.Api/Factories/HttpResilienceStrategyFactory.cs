using Polly.Extensions.Http;
using Polly.Timeout;

namespace Polly.Demo.Api.Factories;

public class HttpResilienceStrategyFactory : IResilienceStrategyFactory
{
    public IAsyncPolicy<HttpResponseMessage> CreateHttpWaitAndRetryStrategy()
    {
        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(2);

        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(1));
        return Policy.WrapAsync(retryPolicy, timeoutPolicy);
    }
}

public interface IResilienceStrategyFactory
{
    IAsyncPolicy<HttpResponseMessage> CreateHttpWaitAndRetryStrategy();
}