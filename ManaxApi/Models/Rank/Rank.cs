// ReSharper disable PropertyCanBeMadeInitOnly.Global

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ManaxApi.Models.Rank;

[Index(nameof(Value), IsUnique = true)]
[Index(nameof(Name), IsUnique = true)]
public class Rank
{
    public long Id { get; set; }
    public int Value { get; set; }
    [MaxLength(128)] public string Name { get; set; }
}