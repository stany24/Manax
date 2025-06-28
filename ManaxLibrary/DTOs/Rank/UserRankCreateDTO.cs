// ReSharper disable InconsistentNaming
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using System.ComponentModel.DataAnnotations;

namespace ManaxLibrary.DTOs.Rank;

public class UserRankCreateDTO
{
    [Required] public long SerieId{ get; set; }
    [Required] public long RankId { get; set; }
}