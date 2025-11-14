// ReSharper disable PropertyCanBeMadeInitOnly.Global

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using System.ComponentModel.DataAnnotations;
using ManaxLibrary.DTO.Rank;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Models.Rank;

[Index(nameof(Value), IsUnique = true)]
[Index(nameof(Name), IsUnique = true)]
public class Rank
{
    public long Id { get; set; }
    public int Value { get; set; }
    [MaxLength(128)] public string Name { get; set; }
    
    public RankDto ToDto()
    {
        return new RankDto
        {
            Id = Id,
            Value = Value,
            Name = Name
        };
    }

    public static Rank Create(RankCreateDto rankCreate)
    {
        return new Rank
        {
            Value = rankCreate.Value,
            Name = rankCreate.Name
        };
    }

    public void Update(RankUpdateDto rankUpdate)
    {
        Value = rankUpdate.Value;
        Name = rankUpdate.Name;
    }
}