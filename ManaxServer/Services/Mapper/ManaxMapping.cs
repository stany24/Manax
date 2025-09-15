using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.Issue.Automatic;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxLibrary.DTO.Library;
using ManaxLibrary.DTO.Rank;
using ManaxLibrary.DTO.Read;
using ManaxLibrary.DTO.SavePoint;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.DTO.Tag;
using ManaxLibrary.DTO.User;
using ManaxServer.Models.Chapter;
using ManaxServer.Models.Issue.Automatic;
using ManaxServer.Models.Issue.Reported;
using ManaxServer.Models.Library;
using ManaxServer.Models.Rank;
using ManaxServer.Models.Read;
using ManaxServer.Models.SavePoint;
using ManaxServer.Models.Serie;
using ManaxServer.Models.Tag;
using ManaxServer.Models.User;

namespace ManaxServer.Services.Mapper;

public class ManaxMapping : Mapping
{
    public ManaxMapping()
    {
        CreateMap<User, UserDto>();
        CreateMap<UserCreateDto, User>();
        CreateMap<UserUpdateDto, User>();

        CreateMap<Serie, SerieDto>();
        CreateMap<SerieCreateDto, Serie>();
        CreateMap<SerieUpdateDto, Serie>();

        CreateMap<Library, LibraryDto>();
        CreateMap<LibraryCreateDto, Library>();
        CreateMap<LibraryUpdateDto, Library>();

        CreateMap<ReportedIssueChapter, ReportedIssueChapterDto>();
        CreateMap<ReportedIssueChapterCreateDto, ReportedIssueChapter>();
        CreateMap<ReportedIssueSerie, ReportedIssueSerieDto>();
        CreateMap<ReportedIssueSerieCreateDto, ReportedIssueSerie>();
        CreateMap<AutomaticIssueSerie, AutomaticIssueSerieDto>();
        CreateMap<AutomaticIssueChapter, AutomaticIssueChapterDto>();
        CreateMap<ReportedIssueChapterType, ReportedIssueChapterTypeDto>();
        CreateMap<ReportedIssueSerieType, ReportedIssueSerieTypeDto>();
        CreateMap<ReportedIssueChapterDto, ReportedIssueChapter>();

        CreateMap<Chapter, ChapterDto>();

        CreateMap<Read, ReadDto>();
        CreateMap<ReadCreateDto, Read>();

        CreateMap<Rank, RankDto>();
        CreateMap<RankCreateDto, Rank>();
        CreateMap<UserRank, UserRankDto>();

        CreateMap<SavePointCreateDto, SavePoint>();

        CreateMap<TagCreateDto, Tag>();
        CreateMap<TagUpdateDto, Tag>();
        CreateMap<Tag, TagDto>();
    }
}