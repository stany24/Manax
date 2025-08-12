// filepath: /media/Pgm3/Git/Manax/ManaxServer/Services/INotificationService.cs

using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.Library;
using ManaxLibrary.DTO.Rank;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.DTO.User;

namespace ManaxServer.Services.Notification;

public interface INotificationService
{
    void NotifyUserCreatedAsync(UserDto user);
    void NotifyUserDeletedAsync(long userId);
    
    void NotifySerieCreatedAsync(SerieDto serie);
    void NotifySerieUpdatedAsync(SerieDto serie);
    void NotifySerieDeletedAsync(long serieId);
    
    void NotifyPosterModifiedAsync(long serieId);
    
    void NotifyLibraryCreatedAsync(LibraryDto library);
    void NotifyLibraryUpdatedAsync(LibraryDto library);
    void NotifyLibraryDeletedAsync(long libraryId);
    
    void NotifyChapterAddedAsync(ChapterDto chapter);
    void NotifyChapterRemovedAsync(long chapterId);
    
    void NotifyRankCreatedAsync(RankDto rank);
    void NotifyRankUpdatedAsync(RankDto rank);
    void NotifyRankDeletedAsync(long rankId);
    
    void NotifyRunningTasksAsync(Dictionary<string, int> tasks);
}
