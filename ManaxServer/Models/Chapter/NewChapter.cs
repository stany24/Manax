using ManaxLibrary.DTO.Chapter;

namespace ManaxServer.Models.Chapter;

public class NewChapter
{
    public long SerieId { get; private init; }
    public long UploaderId { get; set; }
    public int Number { get; private init; }
    public string TempPath { get; set; } = string.Empty;

    public static NewChapter FromDto(NewChapterDto chapter)
    {
        return new NewChapter
        {
            SerieId = chapter.SerieId,
            Number = chapter.Number
        };
    }
}