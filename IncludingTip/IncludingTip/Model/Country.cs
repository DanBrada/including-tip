using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Routing;

namespace IncludingTip.Model;

public class Country
{
    [Key] // ISO country code, numeric ID and/or UUID (GUID if u simp for M$) doesn't make sense here
    public string IsoCountryCode { get; set; }
    public string Name { get; set; }
    public string? TipPolicy { get; set; }
    public DateTime updatedAt { get; set; }

    public ICollection<Place> Places { get; set; } = new List<Place>();

    public double GetAverageTip()
    {
        try
        {
            return Places.Average(p => p.GetAverageTip());
        }
        catch
        {
            return 0.0;
        }
    }
    
}