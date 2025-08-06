// ReSharper disable InconsistentNaming
// ReSharper disable PropertyCanBeMadeInitOnly.Global

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.User;

namespace ManaxLibrary.DTO.Read;

public class ReadDto
{
    public long ChapterId { get; set; }
    public DateTime Date { get; set; }
    public ChapterDto Chapter { get; set; }
    public UserDto User { get; set; }
}