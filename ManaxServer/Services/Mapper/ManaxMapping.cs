using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.Issue.Automatic;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxLibrary.DTO.Library;
using ManaxLibrary.DTO.Rank;
using ManaxLibrary.DTO.Read;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.DTO.User;
using ManaxServer.Models.Issue.Automatic;
using ManaxServer.Models.Issue.Reported;
using ManaxServer.Models.Rank;

namespace ManaxServer.Services.Mapper;

public class ManaxMapping : Mapping
{
    public ManaxMapping()
    {

        // Mappings des User
        CreateMap<Models.User.User, UserDto>();
        CreateMap<UserCreateDto, Models.User.User>();
        CreateMap<UserUpdateDto, Models.User.User>();

        // Mappings des Serie
        CreateMap<Models.Serie.Serie, SerieDto>();
        CreateMap<SerieCreateDto, Models.Serie.Serie>();
        CreateMap<SerieUpdateDto, Models.Serie.Serie>();

        // Mappings des Library
        CreateMap<Models.Library.Library, LibraryDto>();
        CreateMap<LibraryCreateDto, Models.Library.Library>();
        CreateMap<LibraryUpdateDto, Models.Library.Library>();

        // Mappings des Issue
        CreateMap<ReportedIssueChapter, ReportedIssueChapterDto>();
        CreateMap<ReportedIssueChapterCreateDto, ReportedIssueChapter>();
        CreateMap<ReportedIssueSerie, ReportedIssueSerieDto>();
        CreateMap<ReportedIssueSerieCreateDto, ReportedIssueSerie>();
        CreateMap<AutomaticIssueSerie, AutomaticIssueSerieDto>();
        CreateMap<AutomaticIssueChapter, AutomaticIssueChapterDto>();

        // Mappings des Chapter
        CreateMap<Models.Chapter.Chapter, ChapterDto>();

        // Mappings des Read
        CreateMap<Models.Read.Read, ReadDto>();
        CreateMap<ReadCreateDto, Models.Read.Read>();

        // Mappings des Rank
        CreateMap<Models.Rank.Rank, RankDto>();
        CreateMap<RankCreateDto,Models.Rank.Rank>();
        CreateMap<UserRank, UserRankDto>();
    }
}