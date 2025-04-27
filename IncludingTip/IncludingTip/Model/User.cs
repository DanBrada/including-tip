using System.ComponentModel.DataAnnotations;

namespace IncludingTip.Model;

public class User
{
    [Key]
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string PasswordHash { get; set; }
    public ICollection<Tip> Tips { get; set; } = new List<Tip>();
    public ICollection<CountryEdit> CountriesEdited { get; set; } = new List<CountryEdit>();
    
}