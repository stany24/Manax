using ManaxLibrary.DTO.Chapter;

namespace ManaxServer.Models.Chapter;

public class NewChapter
{
    public long SerieId { get; private init; }
    public long UploaderId { get; set; }
    public uint Number { get; private init; }
    public byte[] Data { get; init; } = [];

    public static NewChapter FromDto(NewChapterDto chapter)
    {
        return new NewChapter
        {
            SerieId = chapter.SerieId,
            Number = chapter.Number,
            Data = chapter.Data
        };
    }
}