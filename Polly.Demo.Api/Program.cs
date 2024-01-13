using Polly.Demo.Api.Factories;
using Polly.Registry;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<IResilienceStrategyFactory, HttpResilienceStrategyFactory>();
builder.Services.AddHttpClient("TimeService").AddPolicyHandler((sp, req) =>
{
    var strategyFactory = sp.GetRequiredService<IResilienceStrategyFactory>();
    return strategyFactory.CreateHttpWaitAndRetryStrategy();
});

builder.Services.AddControllers();


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
