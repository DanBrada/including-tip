using StackExchange.Redis;

namespace IncludingTip.Services;

public class RedisService
{
    private IDatabase redis;

    public RedisService()
    {
        var redisConnectionConf = new ConfigurationOptions
        {
            EndPoints = { Environment.GetEnvironmentVariable("REDIS_ENDPOINT") ?? "localhost:6379"}
        };

        var redisConnection = ConnectionMultiplexer.Connect(redisConnectionConf);

        redis = redisConnection.GetDatabase();
    }

    public IDatabase GetRedis()
    {
        return redis;
    }


}