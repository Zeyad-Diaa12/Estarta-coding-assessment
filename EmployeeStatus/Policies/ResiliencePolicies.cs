using Microsoft.EntityFrameworkCore;
using Npgsql;
using Polly;
using Polly.Retry;

namespace EmployeeStatus.Policies;

public static class ResiliencePolicies
{
    public static AsyncRetryPolicy DatabaseRetryPolicy { get; } = Policy
        .Handle<NpgsqlException>()
        .Or<DbUpdateException>()
        .Or<TimeoutException>()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
            onRetry: (exception, timeSpan, retryCount, context) =>
            {
                var logger = context.GetLogger();
                logger?.LogWarning(
                    exception,
                    "Database operation failed (Attempt {RetryCount}/3). Retrying in {RetryDelay}s...",
                    retryCount,
                    timeSpan.TotalSeconds);
            });

    public static AsyncRetryPolicy RedisRetryPolicy { get; } = Policy
        .Handle<StackExchange.Redis.RedisException>()
        .Or<TimeoutException>()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
            onRetry: (exception, timeSpan, retryCount, context) =>
            {
                var logger = context.GetLogger();
                logger?.LogWarning(
                    exception,
                    "Redis operation failed (Attempt {RetryCount}/3). Retrying in {RetryDelay}s...",
                    retryCount,
                    timeSpan.TotalSeconds);
            });

    private static ILogger? GetLogger(this Polly.Context context)
    {
        if (context.TryGetValue("Logger", out var logger))
        {
            return logger as ILogger;
        }
        return null;
    }
}
