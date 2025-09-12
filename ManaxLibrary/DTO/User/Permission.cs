namespace ManaxLibrary.DTO.User;

public enum Permission
{
    ReadSeries = 11,
    WriteSeries = 12,
    DeleteSeries = 13,
    
    ReadChapters = 21,
    UploadChapter = 22,
    DeleteChapters = 23,
    
    ReadUsers = 31,
    WriteUsers = 32,
    DeleteUsers = 33,
    
    ReadAllIssues = 41,
    WriteIssues = 42,
    DeleteIssues = 43,
    
    ReadRanks = 51,
    WriteRanks = 52,
    DeleteRanks = 53,
    SetMyRank = 54,
    
    ReadServerSettings = 61,
    WriteServerSettings = 62,
    
    ReadServerStats = 71,
    ReadSelfStats = 72,
    
    ReadSavePoints = 81,
    WriteSavePoints = 82,
    
    ReadLibrary = 91,
    WriteLibrary = 92,
    DeleteLibrary = 93,
    
    MarkChapterAsRead = 101,
    
    ReadTags = 111,
    WriteTags = 112,
    DeleteTags = 113,
    SetSerieTags = 114,
}
