namespace Polly.Demo.Api.Factories;

public interface IHttpRetryStrategyFactory
{
    ResiliencePipeline<HttpResponseMessage> Create();
}