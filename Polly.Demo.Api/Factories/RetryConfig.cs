namespace Polly.Demo.Api.Factories;

public class RetryConfig
{
    // PertAttemptTimeoutSeconds is the timeout for each attempt in seconds
    public int PertAttemptTimeoutSeconds { get; set; }

    // MaxRetryAttempts is the number of retries before giving up
    public int MaxRetryAttempts { get; set; }

    // RetryDelaySeconds is the median delay between retries in seconds
    public int RetryDelaySeconds { get; set; }
}