using Polly;
using Polly.Demo.Api.Factories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.Configure<RetryConfig>(builder.Configuration.GetSection("RetryConfig"));
builder.Services.AddSingleton<IHttpRetryStrategyFactory, HttpRetryStrategyFactory>();
builder.Services.AddHttpClient("ChaoticService").AddResilienceHandler("RetryStrategy", (pipelineBuilder, context) =>
{
    var strategyFactory = context.ServiceProvider.GetRequiredService<IHttpRetryStrategyFactory>();
    pipelineBuilder.AddPipeline(strategyFactory.Create());
});

builder.Services.AddControllers();


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();