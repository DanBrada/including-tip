using IncludingTip.Model;
using Microsoft.EntityFrameworkCore;

namespace IncludingTip.Services;

public class PlaceService(IDbContextFactory<TipApplicationContext> dbFactory, CachingService cache, GenAIService aiService)
{
    public async Task<List<Place>> GetPlacesByCountry(string code)
    {
        var norm = CountriesService.NormalizeCountryCode(code);

        var cacheKey = cache.Get<List<Place>>($"countries:{norm}:places");

        if (cacheKey is not null)
        {
            Console.WriteLine("Places pulled from cache");
            return cacheKey;
        }

        var db = await dbFactory.CreateDbContextAsync();

        var countries = await db.Places
            .Where(p => p.CountryCode == norm)
            .Include(p => p.Tips)
            .ThenInclude(t => t.Author)
            .ToListAsync();

        cache.Put($"countries:{norm}:places", countries);

        return countries;
    }

    public async Task<string> GetAiSumamry(Place place) => await aiService.GetRecentPlaceTippingSummary(place);


    public async Task<Place?> LoadPlace(int id)
    {
        var cacheKey = cache.Get<Place>($"places:{id}");
        if (cacheKey is not null )
        {
            Console.WriteLine("Pulling place from cache");
            return cacheKey;
        }
        
        var db = await dbFactory.CreateDbContextAsync();

        var place = await db.Places.Include(p => p.Tips).ThenInclude(t => t.Author).Where(p => p.Id == id).FirstAsync();
        
        cache.Put($"places:{id}", place);
        return place;
    }
}