using Polly;
using Polly.Retry;

namespace Emp.Api.PollyPolicies;

public class ClientPolicy
{
    public AsyncRetryPolicy<HttpResponseMessage> ImmediateHttpRetryPolicy { get; }
    public AsyncRetryPolicy<HttpResponseMessage> LinearHttpRetryPolicy { get; }
    public AsyncRetryPolicy<HttpResponseMessage> ExponentialHttpRetryPolicy { get; }

    private const int retryCount = 5;
    private const int sleepDurationSecondsInBetween = 3;

    public ClientPolicy()
    {
        ImmediateHttpRetryPolicy = Policy.HandleResult<HttpResponseMessage>(            // if failure - immediately retry n times
            response => !response.IsSuccessStatusCode)
            .RetryAsync(retryCount);

        LinearHttpRetryPolicy = Policy.HandleResult<HttpResponseMessage>(               // if failure - retry n times, In between sleep 3 seconds
            response => !response.IsSuccessStatusCode)
            .WaitAndRetryAsync(retryCount, retyAttempt => TimeSpan.FromSeconds(sleepDurationSecondsInBetween));

        ExponentialHttpRetryPolicy = Policy.HandleResult<HttpResponseMessage>(               // if failure - retry 3 times, In between sleep 1, 4, 9, 16, 25
            response => !response.IsSuccessStatusCode)
            .WaitAndRetryAsync(3, retyAttempt => TimeSpan.FromSeconds(Math.Pow(2, retyAttempt)));
    }
}
