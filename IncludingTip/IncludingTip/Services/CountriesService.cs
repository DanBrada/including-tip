using Bia.Countries.Iso3166;
using IncludingTip.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding;
using DbCountry = IncludingTip.Model.Country;
using IsoCountry = Bia.Countries.Iso3166.Country;
using CzCountry = ISO3166CZ.Country;

namespace IncludingTip.Services;

public class CountriesService(
    IDbContextFactory<TipApplicationContext> DatabaseFactory,
    NominatimService Nominatim,
    CachingService cache,
    GenAIService genAI)
{
    public record CountryContext(IsoCountry IsoCountry, DbCountry? DbCountry);

    public record CountryStats(double AvgTip, int Reviews, int Places);

    public static string NormalizeCountryCode(string code)
    {
        if (code.Length != 2)
            throw new Exception("Please provide valid country code");

        return code.ToUpper();
    }

    public async Task<CountryContext?> QueryCountry(string isoCode)
    {
        var normalizedCode = NormalizeCountryCode(isoCode);

        var cacheEntry = cache.Get<CountryContext>($"countries:{normalizedCode}");
        if (cacheEntry is not null)
        {
            Console.WriteLine("Pulled country context from cache");
            return cacheEntry;
        }

        var isoCountry = Countries.GetCountryByAlpha2(normalizedCode);

        if (isoCountry is null)
            return null;

        var databaseContext = await DatabaseFactory.CreateDbContextAsync();
        var dbCountry = await databaseContext.Countries
            .Include(c=>c.Places)
            .ThenInclude(p=>p.Tips)
            .FirstOrDefaultAsync(c=>c.IsoCountryCode == normalizedCode);

        var country = new CountryContext(isoCountry, dbCountry);
        cache.Put($"countries:{normalizedCode}", country);
        return country;
    }

    public Task<(double, double)> GetExpandedCountryData(IsoCountry country) =>
        Nominatim.GetCountryLatLong(country);

    public async Task<string> GetRecentAISummary(DbCountry country) =>
        await genAI.GetRecentCountryTippingSummary(country);

    public CzCountry GetCzechCountry(IsoCountry country)
    {
        var cacheEntry = cache.Get<CzCountry>($"countries:{country.Alpha2}:czech");
        if (cacheEntry is not null)
            return cacheEntry;
        var candidates = CzCountry.GetCountries().Where(c => c.Numeric == country.Numeric).ToArray();

        if (candidates.Length != 1)
            throw new Exception("Invalid number of candidates in query");

        var czc = candidates[0];

        if (czc is not null)
        {
            cache.Put($"countries:{country.Alpha2}:czech", czc);
        }

        return czc;
    }

    public async Task SaveCountry(DbCountry country)
    {
        var db = await DatabaseFactory.CreateDbContextAsync();

        // I do this because i might have pulled the country from cache, so it's not the tracked entity
        var ctry = await db.Countries.FindAsync(country.IsoCountryCode);

        if (ctry is null)
            return;

        ctry.TipPolicy = country.TipPolicy;
        ctry.updatedAt = DateTime.UtcNow;
        // and ofc invalidate cache entry
        cache.Invalidate($"countries:{NormalizeCountryCode(country.IsoCountryCode)}");

        await db.SaveChangesAsync();
    }

    public CountryStats GetCountryStats(DbCountry country)
    {
        var cacheKey = cache.Get<CountryStats>($"countries:{country.IsoCountryCode}:stats");

        if (cacheKey is not null)
            return cacheKey;
        
        var averageTip = country.GetAverageTip();
        var ratings = country.Places.Sum(p => p.Tips.Count);
        var places = country.Places.Count;


        var res =  new CountryStats(averageTip, ratings, places);
        
        cache.Put($"countries:{country.IsoCountryCode}:stats", res);

        return res;
    }

    public async Task<DbCountry?> CreateNewCountry(string code, string? tippingPolicy = null)
    {
        var norm = NormalizeCountryCode(code);

        var IsoCountry = Countries.GetCountryByAlpha2(norm);

        var country = new DbCountry()
        {
            IsoCountryCode = norm,
            Name = IsoCountry.ActiveDirectoryName,
            TipPolicy = tippingPolicy,
            updatedAt = DateTime.UtcNow
        };

        var db = await DatabaseFactory.CreateDbContextAsync();

        await db.Countries.AddAsync(country);
        await db.SaveChangesAsync();

        cache.Invalidate($"countries:{norm}");
        
        return await db.Countries.FindAsync(norm);
    }
}