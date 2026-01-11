// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using ManaxLibrary.DTO.Chapter;
using ManaxServer.Settings;

namespace ManaxServer.Models.Chapter;

public class Chapter
{
    public long Id { get; set; }
    public long SerieId { get; set; }
    [ForeignKey(nameof(SerieId))] public Serie.Serie Serie { get; set; } = null!;
    public long UploaderId { get; set; }
    [ForeignKey(nameof(UploaderId))] public User.User Uploader { get; set; } = null!;
    public uint Number { get; set; }
    public uint PageNumber { get; set; }
    public DateTime Creation { get; set; }
    public DateTime LastModification { get; set; }

    public string Path()
    {
        return System.IO.Path.Combine(Serie.SavePath,
            Number + "." + SettingsManager.DataDto.ArchiveFormat.ToString().ToLower(CultureInfo.InvariantCulture));
    }

    public ChapterDto ToDto()
    {
        return new ChapterDto
        {
            Id = Id,
            SerieId = SerieId,
            Number = Number,
            PageNumber = PageNumber,
            Creation = Creation,
            LastModification = LastModification
        };
    }
    
    public bool DtoEquals(ChapterDto dto)
    {
        return Id == dto.Id &&
               SerieId == dto.SerieId &&
               Number == dto.Number &&
               PageNumber == dto.PageNumber &&
               Creation == dto.Creation &&
               LastModification == dto.LastModification;
    }
    
    public static Chapter FromDto(NewChapter dto)
    {
        return new Chapter
        {
            SerieId = dto.SerieId,
            UploaderId = dto.UploaderId,
            Number = dto.Number,
            Creation = DateTime.UtcNow,
            LastModification = DateTime.UtcNow,
        };
    }
}