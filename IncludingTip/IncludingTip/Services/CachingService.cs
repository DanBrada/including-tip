using Newtonsoft.Json;
using StackExchange.Redis;

namespace IncludingTip.Services;

public class CachingService(RedisService redisFactory)
{
    private IDatabase redis = redisFactory.GetRedis();

    public void Put(string key, string value, TimeSpan? expiry = null)
    {
        var exp = expiry ?? TimeSpan.FromMinutes(10);
        
        redis.StringSet($"cache:{key}", value, exp, When.Always, CommandFlags.None);
    }

    public void Put<Tval>(string key, Tval value, TimeSpan? expiry = null) where Tval: class?
    {
        if (value is null)
            return;
        
        // string stringVal = JsonConvert.SerializeObject(value);
        var serializeSettings = new JsonSerializerSettings()
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
        };

        var stringVal = JsonConvert.SerializeObject(value, serializeSettings);
        
        
        Put(key, stringVal, expiry);
    }

    public string? Get(string key)
    {
        return redis.StringGet($"cache:{key}");
    }

    public TVal? Get<TVal>(string key) where TVal: class?
    {
        string? val = Get(key);


        if (val is null)
            return null;

        return JsonConvert.DeserializeObject<TVal>(val);
    }

    public void Invalidate(string key)
    {
        redis.KeyDelete($"cache:{key}");
    }
}