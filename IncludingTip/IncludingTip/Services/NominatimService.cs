using System.Net;
using Newtonsoft.Json;

namespace IncludingTip.Services;

using IsoCountry = Bia.Countries.Iso3166.Country;

public class NominatimService(HttpClient http, CachingService cacheService)
{
    record LookupResultImportantKeys(double lat, double lon, int place_rank);
    
    
    public async Task<(double, double)> GetCountryLatLong(IsoCountry country)
    {
        var cacheEntry = cacheService.Get<LookupResultImportantKeys>($"countries:{country.Alpha2}:country");
        if(cacheEntry is not null)
        {
            return (cacheEntry.lat, cacheEntry.lon);
        }
        
        Console.WriteLine($"URL: https://nominatim.openstreetmap.org/?q=${country.ShortName}&featuretype=country&format=jsonv2");
        
        http.DefaultRequestHeaders.Add("User-Agent", "IncludingTip/0.1-dev");
        
        var result = await http.GetAsync($"https://nominatim.openstreetmap.org/?q=${country.ShortName}&featuretype=country&format=jsonv2", HttpCompletionOption.ResponseContentRead);

        if (result.StatusCode != HttpStatusCode.OK)
        {
            throw new Exception("Cannot query Data");
        }
        
        Console.WriteLine("-------------------RESULT-------------------");
        Console.WriteLine(await result.Content.ReadAsStringAsync());
        Console.WriteLine("-----------------END RESULT-----------------");

        var parsedResult = JsonConvert.DeserializeObject<List<LookupResultImportantKeys>>(await result.Content.ReadAsStringAsync())?.Where(p=>p.place_rank == 4).ToList();

        if (parsedResult.Count > 1)
            throw new Exception("Unambiguous lookup result");
        else if (parsedResult.Count < 1)
            throw new Exception("Nothing found");

        var apiCountry = parsedResult[0];
        cacheService.Put($"countries:{country.Alpha2}:country", apiCountry); // cache the result, so I don't need to spam nominatim

        return (apiCountry.lat, apiCountry.lon);
    }
}