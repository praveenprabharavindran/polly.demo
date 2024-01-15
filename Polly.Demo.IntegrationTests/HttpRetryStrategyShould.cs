using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Polly.Demo.Api.Factories;
using Polly.Timeout;

namespace Polly.Demo.IntegrationTests;

public class HttpRetryStrategyShould
{
    [Fact]
    public async Task RetryWhenTimeoutExpires()
    {
        // Arrange
        const int expectedRetryCount = 3;
        var responseDelay = TimeSpan.FromSeconds(1.5);
        var retryConfig = new RetryConfig
        {
            PertAttemptTimeoutSeconds = 1,
            MaxRetryAttempts = expectedRetryCount - 1,
            RetryDelaySeconds = 1
        };

        var mockOptions = Options.Create(retryConfig);
        var logger = new NullLogger<HttpRetryStrategyFactory>();
        var httpRetryStrategyFactory = new HttpRetryStrategyFactory(logger, mockOptions);
        var retryPolicy = httpRetryStrategyFactory.Create();

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        // Setup Protected method on HttpMessageHandler mock.    
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                Task.Delay(responseDelay, token).Wait(token);
                return new HttpResponseMessage(HttpStatusCode.OK);
            });


        // Create an HttpClient with the policy registry
        var services = new ServiceCollection();
        const string mockHttpClientName = "MockHttpClient";
        const string dummyUrl = "http://dummy.url";
        services.AddHttpClient(mockHttpClientName)
            .ConfigurePrimaryHttpMessageHandler(() => mockHttpMessageHandler.Object)
            .AddResilienceHandler("RetryHandler",
                builder => { builder.AddPipeline(httpRetryStrategyFactory.Create()); });

        // Build the service provider
        var serviceProvider = services.BuildServiceProvider();

        // Use the service provider to create a new HttpClient instance
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient(mockHttpClientName);

        // Act
        await Assert.ThrowsAsync<TimeoutRejectedException>(() => httpClient.GetAsync(dummyUrl));

        // Assert
        mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Exactly(expectedRetryCount),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }
}