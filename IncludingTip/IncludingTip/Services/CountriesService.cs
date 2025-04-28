using Bia.Countries.Iso3166;
using IncludingTip.Model;
using Microsoft.EntityFrameworkCore;
using DbCountry = IncludingTip.Model.Country;
using IsoCountry = Bia.Countries.Iso3166.Country;
using CzCountry = ISO3166CZ.Country;

namespace IncludingTip.Services;

public class CountriesService(IDbContextFactory<TipApplicationContext> DatabaseFactory, NominatimService Nominatim)
{
    public record CountryContext(IsoCountry IsoCountry, DbCountry? DbCountry);

    public static string NormalizeCountryCode(string code)
    {
        if (code.Length != 2)
            throw new Exception("Please provide valid country code");

        return code.ToUpper();
    }

    public CountryContext? QueryCountry(string isoCode)
    {
        var normalizedCode = NormalizeCountryCode(isoCode);

        var isoCountry = Countries.GetCountryByAlpha2(normalizedCode);

        if (isoCountry == null)
            return null;

        var databaseContext = DatabaseFactory.CreateDbContext();
        return new CountryContext(isoCountry, databaseContext.Countries.Find(normalizedCode));
    }

    public Task<(double, double)> GetExpandedCountryData(IsoCountry country) =>
        Nominatim.GetCountryLatLong(country);

    public CzCountry GetCzechCountry(IsoCountry country)
    {
        var candidates = CzCountry.GetCountries().Where(c => c.Numeric == country.Numeric).ToArray();

        if (candidates.Length != 1)
            throw new Exception("Invalid number of candidates in query");

        return candidates[0];
    }
}