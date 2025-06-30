namespace ManaxApi.Models.Issue;

public class ChapterIssueType
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
}

public enum ChapterIssueTypeEnum
{
    NotAllWebp,
    ImageTooBig,
    ImageTooSmall,
    CouldNotOpen,
    MissingPage,
    BadPageNaming
}