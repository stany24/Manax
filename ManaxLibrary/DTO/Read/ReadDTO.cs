// ReSharper disable InconsistentNaming
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace ManaxLibrary.DTO.Read;

public class ReadDto
{
    public long ChapterId { get; set; }
    public int Page { get; set; }
    public DateTime Date { get; set; }
    public long UserId { get; set; }
}