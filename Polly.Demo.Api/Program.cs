using Polly.Demo.Api.Factories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.Configure<RetryConfig>(builder.Configuration.GetSection("RetryConfig"));
builder.Services.AddSingleton<IHttpRetryStrategyFactory, HttpRetryStrategyFactory>();
builder.Services.AddHttpClient("TimeService").AddPolicyHandler((sp, req) =>
{
    var strategyFactory = sp.GetRequiredService<IHttpRetryStrategyFactory>();
    return strategyFactory.Create();
});

builder.Services.AddControllers();


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
