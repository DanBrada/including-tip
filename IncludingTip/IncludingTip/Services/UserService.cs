using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using IncludingTip.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using NSec.Cryptography;

namespace IncludingTip.Services;

public class UserService(IDbContextFactory<TipApplicationContext> dbFactory, IHttpContextAccessor ctxAccessor)
{
    private const int HASH_ROUNDS = 12;

    private Argon2id SetupHashing()
    {
        return PasswordBasedKeyDerivationAlgorithm.Argon2id(new()
        {
            DegreeOfParallelism = 1,
            MemorySize = 64 * 1024, // 64 MiB
        });
    }

    private string HashPassword(string password, byte[]? presetSalt = null)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var salt = presetSalt ?? RandomNumberGenerator.GetBytes(16);
        var argon2Id = SetupHashing();

        var hash = argon2Id.DeriveBytes(passwordBytes, salt, HASH_ROUNDS);

        return $"{Convert.ToBase64String(hash)}${Convert.ToBase64String(hash)}";
    }

    private bool VerifyPassword(string password, string passwordHash)
    {
        var hashParts = passwordHash.Split("$");
        var salt = Convert.FromBase64String(hashParts[0]);
        var hash = Convert.FromBase64String(hashParts[1]);

        var hashedPw = HashPassword(password, salt);
        var hashedPwHash = Convert.FromBase64String(hashedPw.Split("$")[1]);

        return CryptographicOperations.FixedTimeEquals(hash, hashedPwHash);
    }


    public async Task<User> SignUp(string username, string password)
    {
        var pwHash = HashPassword(password);

        var db = await dbFactory.CreateDbContextAsync();

        var users = await db.Users.FindAsync([new { UserName = username }]);

        if (users is not null)
            throw new Exception("User with this username already exists");


        var newUser = new User()
        {
            UserId = Guid.CreateVersion7(),
            UserName = username,
            PasswordHash = pwHash
        };

        var addResult = (await db.Users.AddAsync(newUser)).Entity;
        await db.SaveChangesAsync();
        await Login(username, password);

        return addResult;
    }


    public async Task<User> Login(string username, string password)
    {
        var db = await dbFactory.CreateDbContextAsync();

        var user = await db.Users.FindAsync([new { UserName = username }]);

        if (user is null || !VerifyPassword(password, user.PasswordHash))
        {
            throw new AuthenticationException("Wrong credentials");
        }

        var claims = new List<Claim>()
        {
            new("Id", user.UserId.ToString()),
            new(ClaimTypes.Name, user.UserName),
        };


        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var ctx = ctxAccessor.HttpContext;
        if (ctx is null)
            throw new Exception("Not ran in HTTP(s) context");
        
        await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), new AuthenticationProperties());

        return user;
    }
}