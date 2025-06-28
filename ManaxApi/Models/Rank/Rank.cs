using Microsoft.EntityFrameworkCore;

namespace ManaxApi.Models.Rank;

[Index(nameof(Value), IsUnique = true)]
[Index(nameof(Name), IsUnique = true)]
public class Rank
{
    public long Id { get; set; }
    public int Value { get; set; }
    public string Name { get; set; }
}