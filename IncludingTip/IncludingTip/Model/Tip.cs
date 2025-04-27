using System.ComponentModel.DataAnnotations;

namespace IncludingTip.Model;

public class Tip
{
    [Key]
    public Guid Id { get; set; }
    
    public string Title { get; set; }
    public int Percent { get; set; }
    public string? Experience { get; set; }

    public Guid AuthorId { get; set; }
    public User? Author { get; set; }
    public int PlaceId { get; set; }
    public Place Place { get; set; }
}