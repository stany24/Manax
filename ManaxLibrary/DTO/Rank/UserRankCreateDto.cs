using System.ComponentModel.DataAnnotations;

namespace ManaxLibrary.DTO.Rank;

public class UserRankCreateDto
{
    [Required] public long SerieId { get; init; }
    [Required] public long RankId { get; init; }
}