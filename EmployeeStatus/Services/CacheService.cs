using EmployeeStatus.Policies;
using Polly;
using StackExchange.Redis;
using System.Text.Json;

namespace EmployeeStatus.Services;

public class CacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<CacheService> _logger;

    public CacheService(IConnectionMultiplexer redis, ILogger<CacheService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var context = new Context
        {
            ["Logger"] = _logger
        };

        try
        {
            return await ResiliencePolicies.RedisRetryPolicy.ExecuteAsync(async (ctx) =>
            {
                var db = _redis.GetDatabase();
                var value = await db.StringGetAsync(key);

                if (value.IsNullOrEmpty)
                {
                    _logger.LogInformation("Cache miss for key: {Key}", key);
                    return default;
                }

                _logger.LogInformation("Cache hit for key: {Key}", key);
                return JsonSerializer.Deserialize<T>(value!);
            }, context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
    {
        var context = new Context
        {
            ["Logger"] = _logger
        };

        try
        {
            await ResiliencePolicies.RedisRetryPolicy.ExecuteAsync(async (ctx) =>
            {
                var db = _redis.GetDatabase();
                var serialized = JsonSerializer.Serialize(value);
                await db.StringSetAsync(key, serialized, ttl);
                _logger.LogInformation("Cache set for key: {Key} with TTL: {TTL}s", key, ttl.TotalSeconds);
            }, context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        var context = new Context
        {
            ["Logger"] = _logger
        };

        try
        {
            await ResiliencePolicies.RedisRetryPolicy.ExecuteAsync(async (ctx) =>
            {
                var db = _redis.GetDatabase();
                await db.KeyDeleteAsync(key);
                _logger.LogInformation("Cache removed for key: {Key}", key);
            }, context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache key: {Key}", key);
        }
    }
}
