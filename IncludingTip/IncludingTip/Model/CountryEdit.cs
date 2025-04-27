namespace IncludingTip.Model;

public class CountryEdit
{
    public int Id { get; set; }
    public Country Country { get; set; }
    public User Author { get; set; }
    public string CountryCode { get; set; }
    public Guid AuthorId { get; set; }
}