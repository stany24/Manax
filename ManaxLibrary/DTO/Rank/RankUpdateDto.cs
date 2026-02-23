using System.ComponentModel.DataAnnotations;

namespace ManaxLibrary.DTO.Rank;

public class RankUpdateDto
{
    [Required] public long Id { get; init; }
    [Required] public int Value { get; init; }
    [Required] public string Name { get; init; } = string.Empty;

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Name);
    }
}