using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace IncludingTip.Services;

public class SessionService(RedisService RedisFactory): ITicketStore
{
    private IDatabase redis = RedisFactory.GetRedis();

    /// <summary>
    /// Creates a new Session on the redis backend
    /// </summary>
    /// <param name="id">Optionaly set your session ID</param>
    /// <returns></returns>
    public async Task<(string, DateTime)> StartSession(string? id = null)
    {
        var sessionId = id;

        if (sessionId is null) // I mean it's expected you don't provide a sessionID before, but I could want to, so I made it possible.
        {
            byte[] digest = new byte[5];
            new Random().NextBytes(digest);

            sessionId = Convert.ToHexString(digest);
        }

        var expiry = DateTime.Now + TimeSpan.FromDays(14);

        await redis.HashSetAsync($"session:{sessionId}", []);
        await redis.KeyExpireAsync($"session:{sessionId}", expiry); // Expire session in 14 days

        return (sessionId, expiry);
    }

    public async Task PutField(string sessionId, string key, string value)
    {
        await redis.HashSetAsync($"session:{sessionId}", [
            new(key, value)
        ]);
    }

    public async Task<string?> GetField(string sessionId, string key)
    {
        return await redis.HashGetAsync($"session:{sessionId}", key);
    }

    public async Task<HashEntry[]> Get(string sessionId)
    {
        return await redis.HashGetAllAsync($"session:{sessionId}");
    }

    public async Task EndSession(string sessionId)
    {
        await redis.KeyDeleteAsync($"session:{sessionId}");
    }

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var (sessionId, exp) = await StartSession();

        var ticketJson = JsonConvert.SerializeObject(ticket);

        await PutField(sessionId, "ticket", ticketJson);

        return sessionId;
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        await PutField(key, "ticket", JsonConvert.SerializeObject(ticket));
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        var ticket = await GetField(key, "ticket");

        if (ticket is null)
            return null;
        
        return JsonConvert.DeserializeObject<AuthenticationTicket>(ticket);
    }

    public Task RemoveAsync(string key) => EndSession(key);
}