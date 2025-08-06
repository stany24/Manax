using AutoMapper;
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

namespace ManaxServer.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Mappings entre modèles et Dtos seront configurés ici
        // Exemple : CreateMap<ModeleSource, DtoDestination>();

        // Mappings des User
        CreateMap<User.User, UserDto>();
        CreateMap<UserCreateDto, User.User>();
        CreateMap<UserUpdateDto, User.User>();

        // Mappings des Serie
        CreateMap<Serie.Serie, SerieDto>();
        CreateMap<SerieCreateDto, Serie.Serie>();
        CreateMap<SerieUpdateDto, Serie.Serie>();

        // Mappings des Library
        CreateMap<Library.Library, LibraryDto>();
        CreateMap<LibraryCreateDto, Library.Library>();
        CreateMap<LibraryUpdateDto, Library.Library>();

        // Mappings des Issue
        CreateMap<ReportedIssueChapter, ReportedIssueChapterDto>();
        CreateMap<ReportedIssueChapterCreateDto, ReportedIssueChapter>();
        CreateMap<ReportedIssueSerie, ReportedIssueSerieDto>();
        CreateMap<ReportedIssueSerieCreateDto, ReportedIssueSerie>();
        CreateMap<AutomaticIssueSerie, AutomaticIssueSerieDto>();
        CreateMap<AutomaticIssueChapter, AutomaticIssueChapterDto>();

        // Mappings des Chapter
        CreateMap<Chapter.Chapter, ChapterDto>();

        // Mappings des Read
        CreateMap<Read.Read, ReadDto>();
        CreateMap<ReadCreateDto, Read.Read>();

        // Mappings des Rank
        CreateMap<Rank.Rank, RankDto>();
        CreateMap<RankCreateDto,Rank.Rank>();
        CreateMap<UserRank, UserRankDto>();
    }
}