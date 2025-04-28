using System.Net;
using Newtonsoft.Json;

namespace IncludingTip.Services;

using IsoCountry = Bia.Countries.Iso3166.Country;

public class NominatimService(HttpClient http)
{
    record LookupResultImportantKeys(double lat, double lon);
    
    
    public async Task<(double, double)> GetCountryLatLong(IsoCountry country)
    {
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

        var parsedResult = JsonConvert.DeserializeObject<List<LookupResultImportantKeys>>(await result.Content.ReadAsStringAsync());

        if (parsedResult.Count > 1)
            throw new Exception("Unambiguous lookup result");
        else if (parsedResult.Count < 1)
            throw new Exception("Nothing found");

        var apiCountry = parsedResult[0];

        return (apiCountry.lat, apiCountry.lon);
    }
}