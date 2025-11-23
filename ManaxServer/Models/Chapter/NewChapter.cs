using ManaxLibrary.DTO.Chapter;

namespace ManaxServer.Models.Chapter;

public class NewChapter
{
    public long SerieId { get; set; }
    public long UploaderId { get; set; }
    public int Number { get; set; }
    public string TempPath { get; set; }

    public static NewChapter FromDto(NewChapterDto chapter)
    {
        return new NewChapter
        {
            SerieId = chapter.SerieId,
            Number = chapter.Number,
        };
    }
}