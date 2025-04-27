namespace IncludingTip.Model;

public class Place
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Content { get; set; }
    public double Longitude { get; set; }
    public double Latitutde { get; set; }
    public Country? Country { get; set; } = null;
    public string CountryCode { get; set; }
}